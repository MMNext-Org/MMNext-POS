import { createContext, useContext, useEffect, useState } from 'react';
import { supabase } from '@/utils/supabase';
import type { Product } from '@/types/product';

interface ProductsContextType {
  products: Product[];
  loading: boolean;
  error: string | null;
  refresh: () => void;
}

const ProductsContext = createContext<ProductsContextType | undefined>(undefined);

export const useProducts = () => {
  const context = useContext(ProductsContext);
  if (!context) {
    throw new Error('useProducts must be used within a ProductsProvider');
  }
  return context;
};

export const ProductsProvider = ({ children }: { children: React.ReactNode }) => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchProducts = async () => {
    setLoading(true);
    setError(null);
    try {
      const { data, error } = await supabase
        .from('products')
        .select('id, name, description, price, sku, created_at, stock')
        .order('name', { ascending: true });

      if (error) throw error;
      setProducts((data || []).map((p: any) => ({
        ...p,
        provisional_quantity: p.stock ?? 0,
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

  // Load products on mount
  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  return (
    <ProductsContext.Provider value={{ products, loading, error, refresh: refreshProducts }}>
      {children}
    </ProductsContext.Provider>
  );
};

export default ProductsProvider;