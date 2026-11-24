import { useState } from 'react';
import { useProducts } from '@/context/ProductsContext';
import { useRouter } from 'react-router-dom';
import { useOrderContext } from '@/context/OrderContext';
import { OrderItem } from '@/types';
import { v4 as uuidv4 } from 'uuid';

export default function Products() {
  const { products, loading, error, refresh } = useProducts();
  const router = useRouter();
  const { addOrder } = useOrderContext();

  const handleAddToOrder = (product: any) => {
    // This would be more sophisticated in a real app - open a modal or navigate
    alert(`${product.name} added to order (simulated)`);
  };

  if (loading) return <div className="p-6">Loading products...</div>;
  if (error) return <div className="p-6 text-red-500">{error}</div>;

  return (
    <section className="p-6">
      <h1 className="text-2xl font-bold mb-4 text-coffee-brown">Product Catalog</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {products.map((product) => (
          <div key={product.id} className="border rounded p-4 shadow-sm">
            <h2 className="text-lg font-semibold text-gray-800">{product.name}</h2>
            <p className="text-gray-600">{product.description}</p>
            <p className="text-coffee-brown font-medium">$ {product.price}</p>
            <button
              onClick={() => handleAddToOrder(product)}
              className="mt-2 w-full bg-coffee-brown text-white px-4 py-2 rounded hover:bg-green-600 disabled:opacity-50"
            >
              Add to Order
            </button>
          </div>
        ))}
      </div>

      <div className="mt-6">
        <button
          onClick={refresh}
          className="inline-flex items-center px-4 py-2 border border-coffee-brown text-coffee-brown hover:bg-coffee-brown/10"
        >
          {loading ? 'Refreshing...' : 'Refresh Products'}
        </button>
      </div>
    </section>
  );
}