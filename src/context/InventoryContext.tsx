import { createContext, useContext, useEffect, useState } from 'react';
import { supabase } from '@/utils/supabase';
import type { Product } from '@/types/product';

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
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchProducts = async () => {
    setLoading(true);
    setError(null);
    try {
      const { data, error } = await supabase
        .from('products')
        .select('id, name, description, price, stock, provisional_quantity, organization_id, branch_id')
        .order('name', { ascending: true });
      if (error) throw error;
      setProducts((data || []).map((p: any) => ({
        ...p,
        organizationId: p.organization_id ?? '',
        branchId: p.branch_id ?? '',
      })));
    } catch (err) {
      setError(err.message || 'Failed to fetch products');
    } finally {
      setLoading(false);
    }
  };

  const refreshProducts = () => {
    fetchProducts();
  };

  const updateStock = async (productId: string, delta: number, reason?: string) => {
    const product = products.find((p) => p.id === productId);
    if (!product) {
      setError('Product not found');
      return;
    }
    const newStock = product.stock + delta;
    if (newStock < 0) {
      setError('Insufficient stock to decrement.');
      return;
    }

    try {
      // Cache the authenticated user once instead of calling auth.getUser()
      // twice (once for the org filter, once for the movement user_id).
      const { data: { user: authUser } } = await supabase.auth.getUser();
      const orgId = authUser?.user_metadata?.organization_id ?? '';
      const userId = authUser?.user_metadata?.user_id ?? null;

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
        .eq('organization_id', orgId);

      if (updateError) throw updateError;

      // Optimistically update UI
      setProducts((prev) =>
        prev.map((p) => (p.id === productId ? { ...p, stock: updatedStock } : p)),
      );

      // Record movement (pending)
      await supabase
        .from('stock_movements')
        .insert({
          product_id: productId,
          delta: delta,
          reason: reason,
          user_id: userId,
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

  return (
    <InventoryContext.Provider value={{ products, loading, error, refresh: refreshProducts, updateStock }}>
      {children}
    </InventoryContext.Provider>
  );
};

export default InventoryProvider;
