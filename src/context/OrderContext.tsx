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

  const addOrder = useCallback(async (order: Order) => {
    try {
      // Add to queue items for UI display
      setQueueItems((prev) => [...prev, { id: order.id, order, status: 'pending', createdAt: new Date() }]);

      // Update inventory for all ordered items
      await updateInventoryForOrder(order);

      // Update local state immediately for UI responsiveness
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
  }, []);

  const updateInventoryForOrder = async (order: Order) => {
    try {
      // Extract product IDs and quantities from order items
      const itemsWithQty = order.items.map((item) => ({
        productId: item.productId,
        quantity: item.quantity,
      }));

      for (const { productId, quantity } of itemsWithQty) {
        const { data: product } = await supabase
          .from('products')
          .select('stock')
          .eq('id', productId)
          .single();
        const currentStock = product?.stock ?? 0;
        await supabase
          .from('products')
          .update({ stock: currentStock - quantity })
          .eq('id', productId);
      }
    } catch (err) {
      console.error('Error updating inventory for order:', err);
      throw err;
    }
  };

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