import { useState, useEffect } from 'react';
import { useInventory } from '@/context/InventoryContext';
import { Product } from '@/types/product';

export default function Inventory() {
  const { products, loading, error, refresh, updateStock } = useInventory();
  const [selectedProductId, setSelectedProductId] = useState<string | null>(null);
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

      {loading && <div className="mb-4">Loading products...</div>}
      {error && <p className="text-red-500">{error}</p>}

      <button
        className="mb-4 bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
        onClick={() => setShowModal(true)}
      >
        Add/Edit Stock
      </button>

      <table className="w-full table-auto border-collapse mt-4">
        <thead>
          <tr className="bg-gray-200">
            <th className="border p-2 text-left">Product</th>
            <th className="border p-2 text-center">Stock</th>
            <th className="border p-2 text-center">Actions</th>
          </tr>
        </thead>
        <tbody>
          {products.map((product) => (
            <tr key={product.id} className="border-t">
              <td className="border p-2 text-gray-700">{product.name}</td>
              <td className="border p-2 text-center">{product.stock + product.provisional_quantity}</td>
              <td className="border p-2 text-center">
                <button
                  className="btn-sm bg-blue-600 text-white px-2 py-1 rounded hover:bg-blue-700"
                  onClick={() => openModal(product)}
                >
                  Edit Stock
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {showModal && editingProduct && (

        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center">
          <div className="bg-white p-6 rounded shadow-lg w-96">
            <h2 className="text-xl font-semibold mb-4">
              Update Stock for {editingProduct?.name}
            </h2>
            <input
              type="number"
              className="border p-2 w-full mb-4"
              min="0"
                          value={newStock + editingProduct!.provisional_quantity}
              onChange={(e) => setNewStock(Number(e.target.value))}
            />
            <div className="flex justify-between mt-4">
              <button
                className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700"
                onClick={closeModal}
              >
                Cancel
              </button>
              <button
                className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
                      onClick={() => {
                        const delta = newStock - editingProduct!.stock;
                        if (delta < 0 && newStock < 0) {
                          alert("Stock cannot be negative. Please enter a valid value.");
                          return;
                        }
                        handleStockChange(editingProduct!.id, delta);
                                              }}
              >
                Save
              </button>
            </div>
          </div>
        </div>
                )}
    </section>
  );
}