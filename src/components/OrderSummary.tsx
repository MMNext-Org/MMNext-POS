import React from 'react';

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

interface OrderSummaryProps {
  selectedProductId: string;
  quantity: number;
  selectedProduct: Product | null;
}

const OrderSummary: React.FC<OrderSummaryProps> = ({ selectedProductId, quantity, selectedProduct }) => {
  const selectedPrice = selectedProduct?.price ?? 0;
  const discountRate = quantity > 3 ? 0.10 : 0; // 10% discount for 4+ items
  const subtotal = selectedPrice * quantity * (1 - discountRate);
  const tax = subtotal * 0.08;
  const serviceCharge = 2;
  const total = subtotal + tax + serviceCharge;

  return (
    <div style={{ marginTop: '20px', padding: '15px', border: '1px solid #e0e0e0', borderRadius: '4px', backgroundColor: '#f9f9f9' }}>
      <h3>Order Summary</h3>
      <p>
        <strong>Product:</strong> {selectedProduct?.name ?? 'None'}
      </p>
      <p>
        <strong>Quantity:</strong> {quantity}
      </p>
      <p>
        <strong>Subtotal:</strong> ${selectedPrice.toFixed(2)} × {quantity} = ${subtotal.toFixed(2)}
      </p>
      <p>
        <strong>Discount ({discountRate * 100}% for &gt;3 items):</strong> ${(subtotal * discountRate).toFixed(2)}
      </p>
      <p>
        <strong>Subtotal after discount:</strong> ${subtotal.toFixed(2)}
      </p>
      <p>
        <strong>Tax (8%):</strong> ${tax.toFixed(2)}
      </p>
      <p>
        <strong>Service Charge:</strong> ${serviceCharge.toFixed(2)}
      </p>
      <hr style={{ margin: '15px 0' }} />
      <p>
        <strong>Total:</strong> ${total.toFixed(2)}
      </p>
    </div>
  );
};

export default OrderSummary;