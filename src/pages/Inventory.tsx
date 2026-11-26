import { useState } from 'react';
import { useToast } from '@/components/ui/Toast';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';
import { Button } from '@/components/ui/Button';
import { useInventory } from '@/context/InventoryContext';
import type { Product } from '@/types/product';

export default function Inventory() {
  const { products, loading, error, updateStock } = useInventory();
  const { toast } = useToast();
  const [newStock, setNewStock] = useState<number>(0);
  const [showModal, setShowModal] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);

  // Open modal to edit stock
  const openModal = (product: Product) => {
    setEditingProduct(product);
    setNewStock(product.stock);
    setShowModal(true);
  };

  const closeModal = () => {
    setEditingProduct(null);
    setShowModal(false);
  };

  // Handle stock change (positive or negative delta)
  const handleStockChange = async (productId: string, delta: number) => {
    await updateStock(productId, delta);
  };

  // Simple UI for online status
  const isOnline = navigator.onLine;
  const statusMessage = isOnline ? 'Online' : 'Offline - changes queued';

  return (
    <section className="p-6">
      <h1 className="text-2xl font-bold mb-4 text-coffee-brown">Inventory Management</h1>
      <div className="mb-4 text-sm text-gray-600 flex items-center">
        <span>{statusMessage}</span>
      </div>

      {loading && (
        <div className="mb-4 flex items-center gap-2">
          <LoadingSpinner size="sm" message="Loading products…" />
        </div>
      )}
      {error && (
        <p className="mb-4 text-sm text-red-500" role="alert" aria-live="polite">
          {error}
        </p>
      )}

      <Button
        onClick={() => setShowModal(true)}
        className="mb-4 bg-green-600 hover:bg-green-700"
        disabled={loading}
      >
        Add/Edit Stock
      </Button>

      <table className="w-full table-auto border-collapse mt-4">
        <thead>
          <tr className="bg-gray-200">
            <th className="border p-2 text-left text-gray-700">Product</th>
            <th className="border p-2 text-center text-gray-700">Stock</th>
            <th className="border p-2 text-center text-gray-700">Actions</th>
          </tr>
        </thead>
        <tbody>
          {products.map((product) => (
            <tr key={product.id} className="border-t">
              <td className="border p-2 text-gray-700">{product.name}</td>
              <td className="border p-2 text-center text-gray-700">
                {product.stock + product.provisional_quantity}
              </td>
              <td className="border p-2 text-center">
                <Button
                  variant="primary"
                  size="sm"
                  onClick={() => openModal(product)}
                  disabled={loading}
                >
                  Edit Stock
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {showModal && editingProduct && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded shadow-lg w-96">
            <h2 className="text-xl font-semibold mb-4 text-gray-900">
              Update Stock for {editingProduct.name}
            </h2>
            <input
              type="number"
              className="border border-gray-300 p-2 w-full mb-4 rounded-md"
              min="0"
              value={newStock + editingProduct.provisional_quantity}
              onChange={(e) => setNewStock(Number(e.target.value))}
            />
            <div className="flex justify-between mt-4 gap-2">
              <Button
                variant="secondary"
                onClick={closeModal}
              >
                Cancel
              </Button>
              <Button
                onClick={() => {
                  const delta = newStock - editingProduct.stock;
                  if (newStock < 0) {
                    toast({ title: 'Invalid stock', description: 'Stock cannot be negative. Please enter a valid value.', type: 'error' });
                    return;
                  }
                  handleStockChange(editingProduct.id, delta);
                }}
              >
                Save
              </Button>
            </div>
          </div>
        </div>
      )}
    </section>
  );
}