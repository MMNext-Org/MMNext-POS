import React from 'react';
import OrderCreationWithSummary from '../components/OrderCreationWithSummary';

const OrderNew: React.FC = () => {
  return (
    <div style={{ padding: '20px', fontFamily: 'Arial, sans-serif' }}>
      <h1>New Order</h1>
      <p>Fill out the form to create a new order. Select a product, set quantity, choose modifiers, and click "Add to Order".</p>
      <OrderCreationWithSummary />
    </div>
  );
};

export default OrderNew;