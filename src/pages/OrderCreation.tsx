import { useState, useEffect, useCallback } from 'react';
import { useOrderContext } from '@/context/OrderContext';
import { useLoyaltyContext } from '@/context/LoyaltyContext';
import { useProducts } from '@/context/ProductsContext';
import { useOfflineSync } from '../utils/offlineSync';
import type { Product } from '@/types/product';
import type { OrderItem, Order } from '@/types';
import { supabase } from '@/utils/supabase';
import { v4 as uuidv4 } from 'uuid';

// Simple offline storage using localStorage
const OFFLINE_ORDERS_KEY = 'offline_orders';

// Loyalty points-to-dollar conversion: 100 points = $1.00 discount
const POINTS_PER_DOLLAR = 100;

export default function OrderCreation() {
  const { addOrder } = useOrderContext();
  const {
    customers,
    searchCustomers,
    getAccount,
    redeemPoints,
  } = useLoyaltyContext();
  const { products, refresh } = useProducts();
  const [selectedProductId, setSelectedProductId] = useState<string | null>(null);
  const [quantity, setQuantity] = useState<number>(1);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const isOnline = useOfflineSync(); // Track online status via hook

  // Loyalty / redemption state
  const [customerSearchQuery, setCustomerSearchQuery] = useState('');
  const [selectedCustomer, setSelectedCustomer] = useState<{
    customer: ReturnType<typeof customers.find>;
    account: ReturnType<typeof getAccount>;
  } | null>(null);
  const [pointsToRedeem, setPointsToRedeem] = useState('');

  // Load products on component mount
  useEffect(() => {
    refresh();
  }, [refresh]);

  // Load selected product data when product ID changes
  useEffect(() => {
    if (selectedProductId) {
      const prod = products.find(p => p.id === selectedProductId) || null;
      setSelectedProduct(prod);
    } else {
      setSelectedProduct(null);
    }
  }, [products, selectedProductId]);

  const searchResults = searchCustomers(customerSearchQuery);

  const handleSelectCustomer = useCallback((customerId: string) => {
    const customer = customers.find(c => c.id === customerId);
    if (!customer) return;
    setSelectedCustomer({ customer, account: getAccount(customerId) });
    setPointsToRedeem('');
  }, [customers, getAccount]);

  const handleClearCustomer = useCallback(() => {
    setSelectedCustomer(null);
    setPointsToRedeem('');
    setCustomerSearchQuery('');
  }, []);

  const discountAmount = selectedCustomer && pointsToRedeem
    ? Math.max(0, parseInt(pointsToRedeem, 10) || 0) / POINTS_PER_DOLLAR
    : 0;

  const subtotal = selectedProduct && quantity
    ? selectedProduct.price * quantity
    : 0;

  const total = Math.max(0, subtotal - discountAmount);

  const canRedeem = Boolean(
    selectedCustomer &&
    pointsToRedeem &&
    parseInt(pointsToRedeem, 10) > 0 &&
    parseInt(pointsToRedeem, 10) <= (selectedCustomer.customer.loyaltyPoints || 0)
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!selectedProductId || quantity <= 0) {
      setError('Please select a product and enter a valid quantity.');
      return;
    }
    if (!selectedProduct) {
      setError('Selected product not found.');
      return;
    }
    if (quantity > selectedProduct.stock) {
      setError('Not enough stock for the selected quantity.');
      return;
    }

    // Validate redemption amount does not exceed available points
    if (selectedCustomer && pointsToRedeem) {
      const pts = parseInt(pointsToRedeem, 10);
      if (pts > (selectedCustomer.customer.loyaltyPoints || 0)) {
        setError('Not enough loyalty points to redeem.');
        return;
      }
    }

    // If offline, store locally first
    if (!isOnline) {
      const orderData: Order = {
        id: uuidv4(),
        organizationId: '',
        branchId: 'default-branch',
        status: 'draft',
        total,
        created_at: new Date().toISOString(),
        items: [],
        customerId: selectedCustomer?.customer.id ?? undefined,
        discountAmount: discountAmount > 0 ? discountAmount : undefined,
        redemptionReference: selectedCustomer?.customer.id ?? undefined,
        subtotal,
      };
      await storeLocally(orderData);
      setError('You are offline. Order created locally and will sync when connection is restored.');
      return;
    }

    setIsSaving(true);

    try {
      const orderId = uuidv4();

      // Step 1: Redeem loyalty points first (two-phase reserve + commit)
      if (selectedCustomer && canRedeem) {
        const points = parseInt(pointsToRedeem, 10);
        const redeemed = await redeemPoints(
          selectedCustomer.customer.id,
          points,
          `Redemption for order ${orderId}`,
          orderId,
        );
        if (!redeemed) {
          setError('Failed to redeem loyalty points. Order not created.');
          return;
        }
      }

      // Step 2: Create order item
      const orderItem: OrderItem = {
        id: uuidv4(),
        productId: selectedProductId,
        quantity,
        unitPrice: selectedProduct.price ?? 0,
      };

      // Step 3: Create order with loyalty discount applied
      const organizationId = (await supabase.auth.getUser()).data.user?.user_metadata?.organization_id || '';
      const order: Order = {
        id: orderId,
        organizationId,
        branchId: 'default-branch',
        status: 'draft',
        total,
        created_at: new Date().toISOString(),
        items: [orderItem],
        customerId: selectedCustomer?.customer.id ?? undefined,
        discountAmount: discountAmount > 0 ? discountAmount : undefined,
        redemptionReference: selectedCustomer ? orderId : undefined,
        subtotal,
      };

      await supabase.from('orders').insert([order]);

      // Decrease product stock
      if (selectedProduct) {
        const newStock = selectedProduct.stock - orderItem.quantity;
        await supabase
          .from('products')
          .update({ stock: newStock })
          .eq('id', selectedProduct.id);
      }

      await addOrder(order); // Update local state
    } catch (err) {
      console.error('Error creating order:', err);
      setError('Failed to create order. Please try again.');
    } finally {
      setIsSaving(false);
    }
  };

  // Store order locally for offline use
  const storeLocally = (orderData: Order) => {
    const stored = JSON.parse(localStorage.getItem(OFFLINE_ORDERS_KEY) || '[]');
    stored.push(orderData);
    localStorage.setItem(OFFLINE_ORDERS_KEY, JSON.stringify(stored));
    return Promise.resolve();
  };

  // Simple UI for online status
  const statusMessage = isOnline
    ? 'Online'
    : 'Offline - Order stored locally';

  const availablePoints = selectedCustomer?.customer.loyaltyPoints || 0;

  return (
    <section className="p-6">
      <h1 className="text-2xl font-bold mb-4 text-coffee-brown">Create New Order</h1>
      <div className="mb-4 text-sm text-gray-600 flex items-center">
        <span>{statusMessage}</span>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6 max-w-2xl">
        {/* Product selection */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Product
          </label>
          <select
            className="w-full px-3 py-2 border rounded text-gray-700 focus:outline-none focus:ring-2 focus:ring-coffee-brown"
            value={selectedProductId}
            onChange={(e) => setSelectedProductId(e.target.value)}
            disabled={isSaving}
          >
            <option value="" disabled>
              Select a product
            </option>
            {products.map((product) => (
              <option key={product.id} value={product.id}>
                {product.name} - ${product.price}
              </option>
            ))}
          </select>
          {error && <p className="text-red-500 text-sm mt-1">{error}</p>}
        </div>

        {/* Quantity */}
        <div className="flex items-center">
          <label className="block text-sm font-medium text-gray-700 mr-2">
            Quantity
          </label>
          <input
            type="number"
            className="w-full px-3 py-2 border rounded text-gray-700 focus:outline-none focus:ring-2 focus:ring-coffee-brown"
            min="1"
            value={quantity}
            onChange={(e) => setQuantity(Number(e.target.value))}
            disabled={isSaving}
          />
        </div>

        {/* Loyalty customer & redemption section */}
        <div className="bg-gray-50 rounded-lg border border-gray-200 p-4">
          <div className="flex items-center gap-2 mb-3">
            <h2 className="text-lg font-bold text-gray-900">Customer &amp; Loyalty</h2>
            {selectedCustomer && (
              <button
                type="button"
                onClick={handleClearCustomer}
                className="text-xs text-gray-500 hover:text-gray-900 underline"
              >
                Change customer
              </button>
            )}
          </div>

          {!selectedCustomer ? (
            <>
              <div className="flex gap-2">
                <input
                  type="text"
                  value={customerSearchQuery}
                  onChange={(e) => setCustomerSearchQuery(e.target.value)}
                  placeholder="Search customer by name, email, or phone..."
                  className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                  disabled={isSaving}
                />
                <button
                  type="button"
                  onClick={() => setCustomerSearchQuery('')}
                  disabled={isSaving || !customerSearchQuery}
                  className="px-3 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-100 disabled:opacity-50"
                >
                  Clear
                </button>
              </div>
              {customerSearchQuery && searchResults.length > 0 && (
                <div className="mt-2 border border-gray-200 rounded-md max-h-48 overflow-y-auto">
                  {searchResults.map((c) => (
                    <button
                      key={c.id}
                      type="button"
                      onClick={() => handleSelectCustomer(c.id)}
                      className="w-full text-left px-4 py-3 hover:bg-gray-50 border-b border-gray-100 last:border-b-0 text-sm"
                    >
                      <div className="font-medium text-gray-900">{c.name}</div>
                      <div className="text-sm text-gray-500">
                        {c.email || c.phone || 'No contact info'}
                      </div>
                    </button>
                  ))}
                </div>
              )}
            </>
          ) : (
            <>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                <div>
                  <p className="text-xs font-semibold text-gray-500 uppercase">Customer</p>
                  <p className="text-gray-900">{selectedCustomer.customer.name}</p>
                </div>
                <div>
                  <p className="text-xs font-semibold text-gray-500 uppercase">Loyalty points balance</p>
                  <p className="text-2xl font-bold text-green-700">{availablePoints}</p>
                </div>
                <div>
                  <p className="text-xs font-semibold text-gray-500 uppercase">Tier</p>
                  <p className="text-gray-900">{selectedCustomer.account?.tier || 'Standard'}</p>
                </div>
              </div>

              <div className="space-y-3">
                <label className="block text-sm font-medium text-gray-700">
                  Points to redeem (100 points = $1.00)
                </label>
                <div className="flex gap-2">
                  <input
                    type="number"
                    value={pointsToRedeem}
                    onChange={(e) => setPointsToRedeem(e.target.value)}
                    min="1"
                    max={availablePoints}
                    placeholder="0"
                    className="flex-1 px-3 py-2 border rounded-md text-gray-700 focus:ring-2 focus:ring-coffee-brown"
                    disabled={isSaving}
                  />
                  <span className="px-3 py-2 border border-gray-300 rounded-r-md text-gray-700 bg-gray-50">
                    max {availablePoints}
                  </span>
                </div>
                <p className="text-xs text-gray-500">
                  Discount: ${discountAmount.toFixed(2)} &nbsp;|&nbsp; Available: {availablePoints} points
                </p>
              </div>
            </>
          )}
        </div>

        {/* Order total breakdown */}
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex justify-between text-sm mb-1">
            <span className="text-gray-600">Subtotal</span>
            <span className="font-medium text-gray-900">${subtotal.toFixed(2)}</span>
          </div>
          {discountAmount > 0 && (
            <div className="flex justify-between text-sm mb-1 text-red-600">
              <span>Loyalty discount ({Math.round(discountAmount * POINTS_PER_DOLLAR)} points)</span>
              <span className="font-medium">-${discountAmount.toFixed(2)}</span>
            </div>
          )}
          <div className="flex justify-between text-lg font-bold border-t border-gray-200 pt-2">
            <span>Total</span>
            <span className="text-coffee-brown">${total.toFixed(2)}</span>
          </div>
        </div>

        <button
          type="submit"
          className="w-full bg-coffee-brown text-white px-4 py-2 rounded hover:bg-green-600 disabled:opacity-50"
          disabled={isSaving}
        >
          {isSaving ? 'Creating...' : 'Submit Order'}
        </button>
      </form>
    </section>
  );
}