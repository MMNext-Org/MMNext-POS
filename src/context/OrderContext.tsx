import { createContext, useContext, useState, useEffect, useCallback } from 'react';

import type { Order } from '@/types';
import { supabase } from '@/utils/supabase';
import { initializePowerSync } from '@/utils/powersyncClient';

export interface QueueItem {
  id: string;
  order: Order;
  status: 'pending' | 'uploading' | 'synced' | 'error';
  createdAt: Date;
}

interface OrderContextType {
  orders: Order[];
  addOrder: (order: Order) => Promise<void>;
  queueItems: QueueItem[];
  error: string | null;
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

  // Initialize PowerSync client and subscribe to orders on mount
  useEffect(() => {
    const setupSync = async () => {
      try {
        await initializePowerSync();
        console.log('PowerSync initialized, subscribing to orders...');
        setLoading(false);
      } catch (err) {
        console.error('Failed to set up PowerSync sync:', err);
        setLoading(false);
      }
    };
    setupSync();
  }, []);

  // Batched parallel inventory updates: fetch variant→product mapping in one
  // query, then fire all stock decrements concurrently instead of N sequential
  // Supabase round-trips (SELECT + UPDATE per variant).
  const updateInventoryForOrder = useCallback(async (order: Order) => {
    try {
      const variantIdsWithQty = order.items.map((item) => ({
        variantId: item.product_variant_id,
        quantity: item.quantity,
      }));

      const { data: variants, error: variantError } = await supabase
        .from('product_variants')
        .select('id, product_id')
        .in('id', variantIdsWithQty.map((v) => v.variantId));

      if (variantError) {
        console.error('Failed to fetch product variant data:', variantError);
        throw variantError;
      }

      const variantToProduct = new Map<string, string>();
      variants.forEach((v: { id: string; product_id: string }) => {
        variantToProduct.set(v.id, v.product_id);
      });

      // Cache org id once for all updates in this order.
      const { data: { user: authUser } } = await supabase.auth.getUser();
      const orgId = authUser?.user_metadata?.organization_id ?? '';

      await Promise.all(
        variantIdsWithQty.map(async ({ variantId, quantity }) => {
          const productId = variantToProduct.get(variantId);
          if (!productId) {
            console.warn(`Variant ${variantId} not found in product_variants table`);
            return;
          }
          const { data: product } = await supabase
            .from('products')
            .select('stock')
            .eq('id', productId)
            .single();
          const currentStock = product?.stock ?? 0;
          await supabase
            .from('products')
            .update({ stock: currentStock - quantity })
            .eq('id', productId)
            .eq('organization_id', orgId);
        })
      );
    } catch (err) {
      console.error('Error updating inventory for order:', err);
      throw err;
    }
  }, []);

  const addOrder = useCallback(async (order: Order) => {
    try {
      setQueueItems((prev) => [...prev, { id: order.id, order, status: 'pending', createdAt: new Date() }]);

      await updateInventoryForOrder(order);

      setOrders((prevOrders) => {
        if (!prevOrders.some((o) => o.id === order.id)) {
          return [...prevOrders, order].sort(
            (a, b) =>
              new Date(b.created_at).getTime() - new Date(a.created_at).getTime(),
          );
        }
        return prevOrders;
      });
    } catch (error) {
      console.error('Error saving order:', error);
      setError(error instanceof Error ? error.message : 'Failed to save order');
      setQueueItems((prev) => [...prev, { id: order.id, order, status: 'error', createdAt: new Date() }]);
      throw error;
    }
  }, [updateInventoryForOrder]);

  if (loading) {
    return <div>Loading orders and synchronizing...</div>;
  }

  return (
    <OrderContext.Provider value={{ orders, addOrder, queueItems, error }}>
      {children}
    </OrderContext.Provider>
  );
};

export default OrderProvider;