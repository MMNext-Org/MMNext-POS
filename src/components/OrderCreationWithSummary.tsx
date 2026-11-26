import React, { useState } from 'react';
import { toast } from '@chakra-ui/react';

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

import OrderSummary from './OrderSummary';

const OrderCreationWithSummary: React.FC = () => {
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

  const isValidCardNumber = (cardNumber: string) => {
    // Simple validation: 16 digits
    return /^\d{16}$/.test(cardNumber);
  };

  const handleAddToOrder = () => {
    // Validate product selection
    if (!selectedProduct) {
          toast.error('Please select a product.');
      return;
    }

    // Validate quantity
    if (quantity <= 0) {
      toast.error('Please enter a valid quantity (greater than 0).');
      return;
    }

    // Validate payment method
    if (!paymentMethod) {
      toast.error('Please select a payment method.');
      return;
    }

    if (paymentMethod === 'cash') {
      // Cash payment validation
      const { total } = calculateTotals();
      if (!cashValid) {
              toast.error('Please enter a valid cash amount.');
        return;
      }
      // Cash payment is immediate
      setPaymentStatus('settled');
            toast.success(`Payment successful! Total: $${total.toFixed(2)}`);
    } else if (paymentMethod === 'card') {
      // Card payment validation
      if (!cardNumber) {
              toast.error('Please enter a card number.');
        return;
      }
      if (!isValidCardNumber(cardNumber)) {
              toast.error('Please enter a valid card number (16 digits).');
        return;
      }
      // Simulate API call
      simulateProviderApi(paymentMethod);
    } else if (paymentMethod === 'qr') {
      // QR payment: no specific validation needed for this simulation
      simulateProviderApi(paymentMethod);
    } else {
      // Invalid payment method
      alert('Invalid payment method.');
      return;
    }

    // Reset form fields but keep product selection
    setQuantity(1);
    setModifiers([]);
    setCashAmount(0);
    setChange(0);
    setCashError('');
    setCashValid(false);
  };

  return (
    <div style={{ maxWidth: '100%', margin: '0 auto', padding: '16px', fontFamily: 'inherit' }}>
      <h1>Create Order</h1>
      <div style={{ marginBottom: '20px' }}>
        <label htmlFor="product-select" style={{ display: 'block', marginBottom: '5px' }}>
          Product:
        </label>
        <select
          id="product-select"
          value={selectedProductId}
          onChange={handleSelectProduct}
          style={{
            width: '100%',
            padding: '8px',
            marginBottom: '10px',
            borderRadius: '4px',
            border: '1px solid #ccc',
          }}
        >
          <option value="">-- Select a product --</option>
          {productOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      <div style={{ marginBottom: '20px' }}>
        <label htmlFor="quantity" style={{ display: 'block', marginBottom: '5px' }}>
          Quantity:
        </label>
        <input
          id="quantity"
          type="number"
          min="1"
          value={quantity}
          onChange={handleQuantityChange}
          style={{
            width: '100%',
            padding: '8px',
            marginBottom: '10px',
            borderRadius: '4px',
            border: '1px solid #ccc',
          }}
        />
      </div>

      <div style={{ marginBottom: '20px' }}>
        <fieldset style={{ border: '1px solid #ccc', padding: '10px' }}>
          <legend>Modifiers:</legend>
          {[ 
            { label: 'Size', value: 'size' }, 
            { label: 'Extra Cheese', value: 'extra_cheese' }, 
            { label: 'Toasted', value: 'toasted' }, 
            { label: 'Spicy', value: 'spicy' }, 
          ].map((mod) => (
            <label key={mod.value} style={{ display: 'block', margin: '5px 0' }}>
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
        selectedProductId={selectedProductId}
        quantity={quantity}
        selectedProduct={selectedProduct}
      />

      {/* Cash Payment Section */}
      {selectedProduct && (
        <div style={{ marginTop: '25px', padding: '15px', border: '1px solid #e0e0e0', borderRadius: '4px' }}>
          <h3>Cash Payment</h3>
          <p>
            <strong>Total Amount Due:</strong> ${selectedProduct.price * quantity * (1 - (quantity > 3 ? 0.10 : 0)).toFixed(2)}
          </p>
          <p>
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
            style={{
              width: '100%',
              padding: '8px',
              marginBottom: '10px',
              borderRadius: '4px',
              border: '1px solid #ccc',
            }}
          />

          {cashError && (
            <p style={{ color: 'red', marginBottom: '10px' }}>{cashError}</p>
          )}

          {cashValid && (
            <p style={{ color: 'green', marginBottom: '10px' }}>
              ✅ Cash received! Change: ${change.toFixed(2)}
            </p>
          )}

          <button
            onClick={handleAddToOrder}
            style={{
              flexGrow: 1,
              backgroundColor: '#28a745',
              color: 'white',
              border: 'none',
              padding: '10px',
              borderRadius: '4px',
              cursor: 'pointer',
            }}
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
            style={{
              backgroundColor: '#6c757d',
              color: 'white',
              border: 'none',
              padding: '10px',
              borderRadius: '4px',
              cursor: 'pointer',
            }}
          >
            Clear
          </button>
        </div>
      )}
    </div>
  );
};

export default OrderCreationWithSummary;