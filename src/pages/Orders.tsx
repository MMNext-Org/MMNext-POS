import { useState, useEffect } from 'react';
import { useOrderContext } from '@/context/OrderContext';
import { useProducts } from '@/context/ProductsContext';
import { useRouter } from 'react-router-dom';
import { v4 as uuidv4 } from 'uuid';

export default function Orders() {
  const { orders, addOrder } = useOrderContext();
  const { products, loading: productsLoading, error: productsError, refresh: refreshProducts } = useProducts();
  const navigate = useRouter();
  const [selectedProductId, setSelectedProductId] = useState<string | null>(null);
  const [quantity, setQuantity] = useState<number>(1);
  const [orderItems, setOrderItems] = useState<any[]>([]);
  const [isGenerating, setIsGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load products for order line item selection
  useEffect(() => {
    if (!productsLoading) refreshProducts();
  }, [productsLoading, refreshProducts]);

  const handleAddProduct = (productId: string) => {
    setSelectedProductId(productId);
  };

  const handleGenerateOrder = async () => {
    if (!selectedProductId || quantity <= 0) {
      setError('Please select a product and enter a valid quantity.');
      return;
    }

    setIsGenerating(true);
    setError(null);

    try {
      const selectedProduct = products.find(p => p.id === selectedProductId);
      if (!selectedProduct) throw new Error('Product not found');

      // Create order item
      const orderItem = {
        id: uuidv4(),
        productId: selectedProductId,
        quantity,
        unitPrice: selectedProduct.price,
      };

      const order = {
        id: uuidv4(),
        organizationId: 'org-1', // TODO: use real org
        branchId: 'branch-1', // TODO: use real branch
        status: 'draft',
        total: orderItem.quantity * orderItem.unitPrice,
        created_at: new Date().toISOString(),
        items: [orderItem],
      };

      await addOrder(order);
      setOrderItems([...orderItems, orderItem]);
      setSelectedProductId(null);
      setQuantity(1);
    } catch (err) {
      console.error('Error generating order:', err);
      setError('Failed to generate order.');
    } finally {
      setIsGenerating(false);
    }
  };

  return (
    <section className="p-6">
      <h1 className="text-2xl font-bold mb-4 text-coffee-brown">Orders</h1>

      <div className="space-y-6">
        {/* Order Item Input Section */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Select Product
          </label>
          <select
            className="w-full px-3 py-2 border rounded text-gray-700 focus:outline-none focus:ring-2 focus:ring-coffee-brown"
            value={selectedProductId}
            onChange={(e) => setSelectedProductId(e.target.value)}
          >
            <option value="" disabled>
              Select a product
            </option>
            {productsLoading
              ? (
                <option disabled>
                  Loading products...
                </option>
              : products.map((product) => (
                <option key={product.id} value={product.id}>
                  {product.name} - ${product.price}
                </option>
              ))}
          </select>
          {error && <p className="text-red-500 text-sm mt-1">{error}</p>}
        </div>

        <div className="flex items-center">
          <label className="block text-sm font-medium text-gray-700 mr-2">
            Quantity
          </label>
          <input
            type="number"
            className="w-full px-3 py-2 border rounded text-gray-700 focus:outline-none focus:ring-2 focus:ring-coffee-brown"
            value={quantity}
            onChange={(e) => setQuantity(Number(e.target.value))}
            min="1"
            disabled={isGenerating}
          />
        </div>

        <button
          onClick={handleGenerateOrder}
          className="w-full bg-coffee-brown text-white px-4 py-2 rounded hover:bg-green-600 disabled:opacity-50"
          disabled={isGenerating}
        >
          {isGenerating ? 'Generating...' : 'Create Order'}
        </button>
      </div>

      {/* Order Items List */}
      <div className="space-y-4">
        <h2 className="text-lg font-semibold text-gray-800">Order Items</h2>
        {orderItems.length === 0 ? (
          <p className="text-gray-500">No items in order.</p>
        ) : (
          <ul className="list-disc pl-6">
            {orderItems.map((item) => (
              <li key={item.id} className="flex justify-between items-center p-3 border-t pt-2">
                <div>
                  <strong>{item.productId}</strong>
                  <span className="text-sm ml-2 text-gray-500">
                    × {item.quantity}
                  </span>
                </div>
                <span className="text-gray-700">${item.unitPrice}</span>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  );
}