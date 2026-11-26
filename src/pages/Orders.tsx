import { useState, useEffect } from 'react';
import { useOrderContext } from '@/context/OrderContext';
import { useProducts } from '@/context/ProductsContext';
import { useNavigate } from 'react-router-dom';
import { v4 as uuidv4 } from 'uuid';
import { EmptyState } from '@/components/ui/EmptyState';
import { useToast } from '@/components/ui/Toast';

export default function Orders() {
  const { orders, addOrder } = useOrderContext();
  const { products, loading: productsLoading, error: productsError, refresh: refreshProducts } = useProducts();
  const navigate = useNavigate();
  const { toast } = useToast();

  // Load products for order line item selection
  useEffect(() => {
    if (!productsLoading) refreshProducts();
  }, [productsLoading, refreshProducts]);

  const [selectedProductId, setSelectedProductId] = useState<string | null>(null);
  const [quantity, setQuantity] = useState<number>(1);
  const [orderItems, setOrderItems] = useState<any[]>([]);
  const [isGenerating, setIsGenerating] = useState(false);

  const handleGenerateOrder = async () => {
    if (!selectedProductId || quantity <= 0) {
      toast({ title: 'Invalid order', description: 'Please select a product and enter a valid quantity.', type: 'error' });
      return;
    }

    setIsGenerating(true);

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
      toast({ title: 'Order created', description: `Order for ${selectedProduct.name} × ${quantity} added.`, type: 'success' });
    } catch (err) {
      console.error('Error generating order:', err);
      toast({ title: 'Failed to generate order', description: err instanceof Error ? err.message : 'Unknown error', type: 'error' });
    } finally {
      setIsGenerating(false);
    }
  };

  return (
    <section className="min-h-screen bg-gray-50 p-4 sm:p-8">
      <main className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold mb-4 text-coffee-brown" id="orders-heading">Orders</h1>

        <div className="space-y-6">
          {/* Order Item Input Section */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label
                htmlFor="product-select"
                className="block text-sm font-medium text-gray-700 mb-1"
              >
                Select Product
              </label>
              <select
                id="product-select"
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-gray-700 focus:ring-2 focus:ring-coffee-brown"
                value={selectedProductId}
                onChange={(e) => setSelectedProductId(e.target.value)}
                aria-label="Select product"
              >
                <option value="" disabled>
                  Select a product
                </option>
                {productsLoading
                  ? (
                    <option disabled>
                      Loading products...
                    </option>
                  ) : products.map((product) => (
                    <option key={product.id} value={product.id}>
                      {product.name} - ${product.price}
                    </option>
                  ))}
              </select>
              {productsError && <p className="text-sm text-red-600 mt-1">{productsError.message || productsError}</p>}
            </div>
            <div className="flex flex-col sm:flex-row items-end sm:items-center gap-2">
              <label
                htmlFor="quantity-input"
                className="block text-sm font-medium text-gray-700 mb-1 sm:mb-0"
              >
                Quantity
              </label>
              <input
                id="quantity-input"
                type="number"
                className="w-full sm:w-24 px-3 py-2 border border-gray-300 rounded-md text-gray-700 focus:ring-2 focus:ring-coffee-brown"
                value={quantity}
                onChange={(e) => setQuantity(Number(e.target.value))}
                min="1"
                disabled={isGenerating}
                aria-label="Quantity"
              />
            </div>
            <button
              onClick={handleGenerateOrder}
              className="w-full sm:w-auto bg-coffee-brown text-white px-4 py-2 rounded hover:bg-green-600 disabled:opacity-50 disabled:cursor-not-allowed"
              disabled={isGenerating || !selectedProductId}
              aria-label={isGenerating ? 'Generating order...' : 'Create order'}
            >
              {isGenerating ? 'Generating...' : 'Create Order'}
            </button>
          </div>

          {/* Order Items List */}
          <div className="space-y-4">
            <h2 className="text-lg font-semibold text-gray-800">Order Items</h2>
            {orderItems.length === 0 ? (
              <EmptyState
                icon="🧾"
                title="No items in order"
                description="Select a product and quantity, then create an order to add items."
              />
            ) : (
              <ul className="list-disc pl-6">
                {orderItems.map((item) => (
                  <li key={item.id} className="flex justify-between items-center p-3 border-t pt-2">
                    <div>
                      <strong>
                        {products.find((p) => p.id === item.productId)?.name || item.productId}
                      </strong>
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
        </div>
      </main>
    </section>
  );
}