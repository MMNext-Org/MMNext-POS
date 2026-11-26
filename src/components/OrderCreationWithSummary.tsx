import React, { useState } from 'react';
import { useToast } from '@/components/ui/Toast';
import OrderSummary from './OrderSummary';

// Define product type
interface Product {
  id: string;
  name: string;
  category: string;
  price: number;
}

// Mock product list (same as in MenuCategoryView)
const mockProducts: Product[] = [
  { id: '1', name: 'Coffee', category: 'Beverages', price: 3.5 },
  { id: '2', name: 'Tea', category: 'Beverages', price: 2.5 },
  { id: '3', name: 'Juice', category: 'Beverages', price: 4.0 },
  { id: '4', name: 'Burger', category: 'Food', price: 8.0 },
  { id: '5', name: 'Fries', category: 'Food', price: 3.5 },
  { id: '6', name: 'Salad', category: 'Food', price: 6.0 },
  { id: '7', name: 'Cake', category: 'Desserts', price: 5.0 },
  { id: '8', name: 'Ice Cream', category: 'Desserts', price: 4.5 },
];

const OrderCreationWithSummary: React.FC = () => {
  const { toast } = useToast();
  const [selectedProductId, setSelectedProductId] = useState<string>('');
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [quantity, setQuantity] = useState<number>(1);
  const [modifiers, setModifiers] = useState<string[]>([]);

  // Cash payment states
  const [cashAmount, setCashAmount] = useState<number>(0);
  const [change, setChange] = useState<number>(0);
  const [cashError, setCashError] = useState<string>('');
  const [cashValid, setCashValid] = useState<boolean>(false);

  const productOptions = mockProducts.map((p) => ({
    value: p.id,
    label: `${p.name} ($${p.price})`,
  }));

  const calculateTotals = () => {
    if (!selectedProduct) return { total: 0, subtotal: 0, tax: 0, serviceCharge: 0, discount: 0 };
    const discountRate = quantity > 3 ? 0.10 : 0;
    const subtotal = selectedProduct.price * quantity * (1 - discountRate);
    const tax = subtotal * 0.08;
    const serviceCharge = 2;
    const total = subtotal + tax + serviceCharge;
    return { total, subtotal, tax, serviceCharge, discount: discountRate };
  };

  const handleSelectProduct = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const id = event.target.value;
    setSelectedProductId(id);
    setSelectedProduct(mockProducts.find((p) => p.id === id) || null);
    // Reset cash states when product changes
    setCashAmount(0);
    setChange(0);
    setCashError('');
    setCashValid(false);
  };

  const handleQuantityChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(event.target.value, 10);
    if (isNaN(value) || value <= 0) {
      setQuantity(1); // Default to 1 if invalid
    } else {
      setQuantity(value);
    }
  };

  const handleModifierChange = (checkboxEvent: React.ChangeEvent<HTMLInputElement>) => {
    if (checkboxEvent.target.checked) {
      setModifiers([...modifiers, checkboxEvent.target.value]);
    } else {
      setModifiers(modifiers.filter((m) => m !== checkboxEvent.target.value));
    }
  };

  const handleAddToOrder = () => {
    // Validate product selection
    if (!selectedProduct) {
      toast({ title: 'Product required', description: 'Please select a product.', type: 'error' });
      return;
    }

    // Validate quantity
    if (quantity <= 0) {
      toast({ title: 'Invalid quantity', description: 'Please enter a valid quantity (greater than 0).', type: 'error' });
      return;
    }

    // Cash payment validation
    const { total } = calculateTotals();
    if (!cashValid) {
      toast({ title: 'Invalid cash amount', description: 'Please enter a valid cash amount.', type: 'error' });
      return;
    }
    // Cash payment is immediate
    toast({ title: 'Payment successful', description: `Total: $${total.toFixed(2)}`, type: 'success' });

    // Reset form fields but keep product selection
    setQuantity(1);
    setModifiers([]);
    setCashAmount(0);
    setChange(0);
    setCashError('');
    setCashValid(false);
  };

  return (
    <div className="max-w-xl mx-auto p-4 font-sans">
      <h1 className="text-xl font-bold mb-4">Create Order</h1>
      <div className="mb-4">
        <label htmlFor="product-select" className="block mb-1 text-sm font-medium">
          Product:
        </label>
        <select
          id="product-select"
          value={selectedProductId}
          onChange={handleSelectProduct}
          className="w-full px-3 py-2 mb-4 rounded border border-gray-300 focus:outline-none focus:ring-2 focus:ring-coffee-brown"
        >
          <option value="">-- Select a product --</option>
          {productOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      <div className="mb-4">
        <label htmlFor="quantity" className="block mb-1 text-sm font-medium">
          Quantity:
        </label>
        <input
          id="quantity"
          type="number"
          min="1"
          value={quantity}
          onChange={handleQuantityChange}
          className="w-full px-3 py-2 mb-4 rounded border border-gray-300 focus:outline-none focus:ring-2 focus:ring-coffee-brown"
        />
      </div>

      <div className="mb-4">
        <fieldset className="border border-gray-300 p-4 rounded">
          <legend className="text-sm font-medium">Modifiers:</legend>
          {[ 
            { label: 'Size', value: 'size' }, 
            { label: 'Extra Cheese', value: 'extra_cheese' }, 
            { label: 'Toasted', value: 'toasted' }, 
            { label: 'Spicy', value: 'spicy' }, 
          ].map((mod) => (
            <label key={mod.value} className="block mb-1">
              <input
                type="checkbox"
                name="modifier"
                value={mod.value}
                checked={modifiers.includes(mod.value)}
                onChange={handleModifierChange}
              />{' '}
              {mod.label}
            </label>
          ))}
        </fieldset>
      </div>

      {/* Order Summary Section */}
      <OrderSummary
        selectedProduct={selectedProduct}
        quantity={quantity}
      />

      {/* Cash Payment Section */}
      {selectedProduct && (
        <div className="mt-6 p-4 border border-gray-200 rounded bg-gray-50">
          <h3 className="text-lg font-bold mb-2">Cash Payment</h3>
          <p className="mb-2">
            <strong>Total Amount Due:</strong> ${Number(selectedProduct.price * quantity * (1 - (quantity > 3 ? 0.10 : 0))).toFixed(2)}
          </p>
          <p className="mb-2">
            <strong>Enter Cash Amount:</strong>
          </p>
          <input
            type="number"
            value={cashAmount}
            onChange={(e) => {
              const amount = parseInt(e.target.value, 10) || 0;
              setCashAmount(amount);

              const { total } = calculateTotals();
              if (amount < total) {
                setCashError('Insufficient cash');
                setChange(0);
                setCashValid(false);
              } else if (amount > 5000) { // max cash limit
                setCashError('Cash amount exceeds limit');
                setChange(0);
                setCashValid(false);
              } else {
                const changeValue = amount - total;
                setChange(changeValue);
                setCashError('');
                setCashValid(true);
              }
            }}
            className="w-full px-3 py-2 mb-2 rounded border border-gray-300 focus:outline-none focus:ring-2 focus:ring-coffee-brown"
          />

          {cashError && (
            <p className="mb-2 text-red-600">{cashError}</p>
          )}

          {cashValid && (
            <p className="mb-2 text-green-600">
              ✅ Cash received! Change: ${change.toFixed(2)}
            </p>
          )}

          <div className="flex gap-2">
            <button
              onClick={handleAddToOrder}
              className="flex-1 bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 cursor-pointer disabled:opacity-50"
            >
              Add to Order & Pay Cash
            </button>

            <button
              onClick={() => {
                setSelectedProductId('');
                setSelectedProduct(null);
                setQuantity(1);
                setModifiers([]);
                setCashAmount(0);
                setChange(0);
                setCashError('');
                setCashValid(false);
              }}
              className="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600 cursor-pointer"
            >
              Clear
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default OrderCreationWithSummary;