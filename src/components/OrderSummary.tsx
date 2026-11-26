import React from 'react';

// Define product type
interface Product {
  id: string;
  name: string;
  category: string;
  price: number;
}

interface OrderSummaryProps {
  selectedProduct: Product | null;
  quantity: number;
}

const OrderSummary: React.FC<OrderSummaryProps> = ({ selectedProduct, quantity }) => {
  const selectedPrice = selectedProduct?.price ?? 0;
  const discountRate = quantity > 3 ? 0.10 : 0; // 10% discount for 4+ items
  const subtotal = selectedPrice * quantity * (1 - discountRate);
  const tax = subtotal * 0.08;
  const serviceCharge = 2;
  const total = subtotal + tax + serviceCharge;

  return (
    <div className="mt-5 p-4 border border-gray-200 rounded-md bg-gray-50">
      <h3 className="font-semibold text-gray-900 mb-2">Order Summary</h3>
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
      <hr className="my-4 border-gray-200" />
      <p>
        <strong>Total:</strong> ${total.toFixed(2)}
      </p>
    </div>
  );
};

export default React.memo(OrderSummary);