import React from 'react';
import OrderCreationWithSummary from '../components/OrderCreationWithSummary';

const OrderNew: React.FC = () => {
  return (
    <div className="p-4 max-w-4xl mx-auto font-sans">
      <h1 className="text-2xl font-bold mb-4 text-coffee-brown">New Order</h1>
      <p className="mb-4 text-gray-700">
        Fill out the form to create a new order. Select a product, set quantity, choose modifiers, and click "Add to Order".
      </p>
      <OrderCreationWithSummary />
    </div>
  );
};

export default OrderNew;