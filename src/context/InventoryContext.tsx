import { createContext, useContext, useEffect, useState } from 'react';
import { useSupabaseClient } from '@/utils/supabase';
import { Product } from '@/types/product';

interface InventoryContextType {
  products: Product[];
  loading: boolean;
  error: string | null;
  refresh: () => void;
  updateStock: (productId: string, delta: number, reason?: string) => Promise<void>;
}

const InventoryContext = createContext<InventoryContextType | undefined>(undefined);

export const useInventory = () => {
  const context = useContext(InventoryContext);
  if (!context) {
    throw new Error('useInventory must be used within a InventoryProvider');
  }
  return context;
};

export const InventoryProvider = ({ children }: { children: React.ReactNode }) => {
  const supabase = useSupabaseClient();
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [offlineQueue, setOfflineQueue] = useState<Record<string, { delta: number; reason: string; timestamp: string }>>({});
  const [onlineStatus, setOnlineStatus] = useState<boolean>(navigator.onLine);

  const OFFLINE_QUEUE_KEY = 'offline_inventory_queue';

  const getOfflineQueue = (): Record<string, { delta: number; reason: string; timestamp: string }> => {
    const stored = localStorage.getItem(OFFLINE_QUEUE_KEY);
    return stored ? JSON.parse(stored) : {};
  };

  const setOfflineQueueState = (queue: Record<string, { delta: number; reason: string; timestamp: string }>) => {
    localStorage.setItem(OFFLINE_QUEUE_KEY, JSON.stringify(queue));
  };

  const fetchProducts = async () => {
    setLoading(true);
    setError(null);
    try {
      const { data, error } = await supabase
        .from('products')
        .select('id, name, description, price, stock, provisional_quantity, organization_id, branch_id')
        .order('name', { ascending: true });
      if (error) throw error;
      setProducts(data || []);
    } catch (err) {
      setError(err.message || 'Failed to fetch products');
    } finally {
      setLoading(false);
    }
  };

  const refreshProducts = () => {
    fetchProducts();
  };

  const updateStock = async (productId: string, delta: number, reason = 'stock adjustment') => {
    const isOnline = navigator.onLine;
    const product = products.find(p => p.id === productId);
    if (!product) {
      setError('Product not found');
      return;
    }
    const newStock = product.stock + delta;
    if (newStock < 0) {
      setError('Insufficient stock to decrement.');
      return;
    }
    if (!isOnline) {
      const queue = getOfflineQueue();
      const entry = queue[productId];
      if (entry) {
        entry.delta += delta;
        entry.reason = (entry.reason ? entry.reason + '; ' : '') + reason;
        entry.timestamp = new Date().toISOString();
      } else {
        queue[productId] = {
          delta,
          reason,
          timestamp: new Date().toISOString(),
        };
      }
      setOfflineQueueState(queue);
      setOfflineQueue(queue);
      setError('Offline: stock change queued');
      return;
    }

    try {
      // Get current stock from DB
      const { data, error: dbError } = await supabase
        .from('products')
        .select('stock')
        .eq('id', productId)
        .single();
      if (dbError) throw dbError;
      const currentStock = data?.stock ?? 0;
      const updatedStock = currentStock + delta;

      const { error: updateError } = await supabase
        .from('products')
        .update({ stock: updatedStock })
        .eq('id', productId)
        .eq('organization_id', (await supabase.auth.getUser()).data.user?.user_metadata?.organization_id ?? '');

      if (updateError) throw updateError;

      // Optimistically update UI
      setProducts(prev => prev.map(p => p.id === productId ? { ...p, stock: updatedStock } : p));

      // Record movement (pending)
      await supabase
        .from('stock_movements')
        .insert({
          product_id: productId,
          delta: delta,
          reason: reason,
          user_id: (await supabase.auth.getUser()).data.user?.user_metadata?.user_id ?? null,
          status: 'pending',
        });
    } catch (err) {
      setError(err.message || 'Failed to update stock');
    }
  };

  // Load products on mount
  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  // Handle online/offline events to manage queue
  useEffect(() => {
      const handleOnline = async () => {
        // Process offline queue when going online
        const queue = getOfflineQueue();
        for (const entry of Object.values(queue)) {
          try {
            const { productId, delta, reason, timestamp } = entry;
            const { data, error: fetchError } = await supabase
              .from('products')
              .select('stock')
              .eq('id', productId)
              .single();

            if (fetchError) {
              console.error('Failed to fetch product stock for offline processing', fetchError);
              continue;
            }

            const currentStock = data?.stock ?? 0;
            const newStock = currentStock + delta;

            if (newStock < 0) {
              console.warn('Insufficient stock for product', productId, 'during offline processing');
              // Optionally handle the error (e.g., mark entry as failed)
              continue;
            }

            await supabase
              .from('products')
              .update({ stock: newStock })
              .eq('id', productId);

            await supabase
              .from('stock_movements')
              .update({ status: 'committed' })
              .eq('product_id', productId);
          } catch (err) {
            console.error('Error processing offline stock change', err);
          }
        }
        // Clear processed queue
      setOfflineQueueState({});
      setOfflineQueue({});
    };

    const handleOffline = () => {
      // Optionally persist queue; already persisted via localStorage
    };

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);
    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, [onlineStatus]);

  return {
    products,
    loading,
    error,
    refresh: refreshProducts,
    updateStock,
  };
}

export default InventoryProvider;
