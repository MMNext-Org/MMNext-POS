import { createContext, useContext, useState, useEffect, useCallback } from 'react';

import { Order } from '@/types';
import { powersyncClient, initializePowerSync } from '@/utils/powersyncClient';
import { updateStock } from '@/context/InventoryContext';
import { supabase } from '@/utils/supabase';
import { Database } from '@powersync/node';

interface OrderContextType {
  orders: Order[];
  addOrder: (order: Order) => Promise<void>;
}

const OrderContext = createContext<OrderContextType | undefined>(undefined);

export const useOrderContext = () => {
  const context = useContext(OrderContext);
  if (!context) {
    throw new Error('useOrderContext must be used within OrderProvider');
  }
  return context;
};

export const OrderProvider = ({ children }: { children: React.ReactNode }) => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [queueItems, setQueueItems] = useState<QueueItem[]>([]);
  const [error, setError] = useState<string | null>(null);

  // Define queue item type
  interface QueueItem {
    id: string;
    order: Order;
    status: 'pending' | 'uploading' | 'synced' | 'error';
    createdAt: Date;
  }

  // Initialize PowerSync client and subscribe to orders on mount
  useEffect(() => {
    const setupSync = async () => {
      try {
        await initializePowerSync();
        console.log('PowerSync initialized, subscribing to orders...');
        
        // Subscribe to the 'orders' collection (assuming this matches the PowerSync stream)
        const subscription = powersyncClient.subscribe('orders', (data) => {
          console.log('Received remote order update:', data);
          // Update local state with remote changes
          setOrders(prevOrders => {
            const existingOrder = prevOrders.find(o => o.id === data.id);
            if (existingOrder) {
              // Merge remote data, preferring remote changes for sync updates
              return prevOrders.map(o => o.id === data.id ? data : o);
            } else {
              // New remote order
              return [...prevOrders, data];
            }
          });
        });
        
        // Listen for sync errors
        powersyncClient.on('sync_error', (errorMsg) => {
          console.error('PowerSync sync error:', errorMsg);
          setError(errorMsg);
          // Optionally add to queue items with error status
          setQueueItems(prev => [...prev, {
            id: 'sync_error',
            order: { id: 'unknown', productId: '', quantity: 0, unitPrice: 0, created_at: new Date().toISOString() },
            status: 'error',
            createdAt: new Date()
          }]);
        });
        
        // Listen for sync progress (optional)
        powersyncClient.on('sync_progress', (progress) => {
          console.log('Sync progress:', progress);
          // Update queue items based on progress if needed
        });
        
        // Fetch initial data by querying the local database or subscribing to initial state
        // For simplicity, we'll query the local SQLite adapter state on init if available
        
                // Fetch initial data by querying the local database or subscribing to initial state
        const localDb = powersyncClient.getLocalDatabase();
        localDb.select('*').then((data) => {
          setOrders((data as Order[]).sort((a, b) => new Date(b.created_at).getTime() - new Date(a.created_at).getTime()));
          setLoading(false);
        }).catch(err => {
          console.error('Error fetching initial orders from local DB:', err);
          setLoading(false);
        });

        return () => {
          subscription.unsubscribe();
          powersyncClient.off('sync_error');
          powersyncClient.off('sync_progress');
        };
      } catch (err) {
        console.error('Failed to set up PowerSync sync:', err);
        setLoading(false);
      }
    };
    setupSync();
  }, []);

  const addOrder = useCallback(async (order: Order) => {
    try {
      // Save the order locally, which queues it for cloud sync automatically
      const savedOrder = await powersyncClient.save(order);
      console.log('Order saved and queued for sync:', savedOrder.id);

      // Add to queue items for UI display
      setQueueItems(prev => [...prev, {
        id: savedOrder.id,
        order: savedOrder,
        status: 'pending',
        createdAt: new Date()
      }]);

      // Update inventory for all ordered items
      await updateInventoryForOrder(order);

      // Update local state immediately for UI responsiveness
      setOrders(prevOrders => {
        // Avoid duplicate if already present due to remote sync callback
        if (!prevOrders.some(o => o.id === savedOrder.id)) {
          return [...prevOrders, savedOrder].sort((a, b) => new Date(b.created_at).getTime() - new Date(a.created_at).getTime());
        }
        return prevOrders;
      });
    } catch (error) {
      console.error('Error saving order with PowerSync:', error);
      setError(error.message || 'Failed to save order');
      setQueueItems(prev => [...prev, {
                        id: order.id,
                        order,
                        status: 'error',
                        createdAt: new Date()
      }]);
      throw error;
    }
  }, [updateInventoryForOrder]);

    const updateInventoryForOrder = useCallback(async (order: Order) => {
      try {
        // Extract product variant IDs and quantities from order items
        const variantIdsWithQty = order.items.map((item) => ({
          variantId: item.product_variant_id,
          quantity: item.quantity,
        }));

        // Fetch product IDs for all variant IDs in one query
        const { data: variants, error: variantError } = await supabase
          .from('product_variants')
          .select('id, product_id')
          .in('id', variantIdsWithQty.map(v => v.variantId));

        if (variantError) {
          console.error('Failed to fetch product variant data:', variantError);
          throw variantError;
        }

        // Create a map variantId => productId
        const variantToProduct = new Map<string, string>();
        variants.forEach((v: { id: string; product_id: string }) => {
          variantToProduct.set(v.id, v.product_id);
        });

        // Fire all variant stock updates in parallel instead of sequentially,
        // avoiding N sequential supabase round-trips (SELECT + UPDATE per variant).
        // `updateStock` is a stable module-level import, so it is intentionally
        // omitted from the dependency array.
        await Promise.all(
          variantIdsWithQty.map(async ({ variantId, quantity }) => {
            const productId = variantToProduct.get(variantId);
            if (!productId) {
              console.warn(`Variant ${variantId} not found in product_variants table`);
              return;
            }
            await updateStock(productId, -quantity);
          })
        );
      } catch (err) {
        console.error('Error updating inventory for order:', err);
        // Re-throw to let addOrder handle the error (setError, queue item)
        throw err;
      }
    }, []);


  if (loading) {
    return <div>Loading orders and synchronizing...</div>;
  }

  return (
    <OrderContext.Provider value={{ orders, addOrder }}>
      {children}
    </OrderContext.Provider>
  );
};

export default OrderProvider;