import { useState } from 'react';

type OrderItem = {
  productId: string;
  name: string;
  quantity: number;
  unitPrice: number;
  total: number;
};

type Order = {
  id: string;
  status: 'pending' | 'processing' | 'completed' | 'paid' | 'voided';
  total: number;
  items: OrderItem[];
  createdAt: string;
  table?: number;
};

export const ReceiptView = ({ order }: { order: Order }) => {
  const [isPrinting, setIsPrinting] = useState(false);

  const handlePrint = () => {
    setIsPrinting(true);
    setTimeout(() => {
      setIsPrinting(false);
      window.print();
    }, 100);
  };

  return (
    <div className="receipt-view">
      <header className="receipt-header">
        <h1>Receipt</h1>
        <button 
          onClick={handlePrint}
          className="print-button"
          disabled={isPrinting}
        >
          {isPrinting ? 'Printing...' : 'Print Receipt'}
        </button>
      </header>

      <div className="receipt-details">
        <div className="receipt-metadata">
          <p><strong>Order #:</strong> {order.id}</p>
          <p><strong>Status:</strong> {order.status}</p>
          <p><strong>Date:</strong> {new Date(order.createdAt).toLocaleString()}</p>
          {order.table && <p><strong>Table:</strong> {order.table}</p>}
        </div>

        <h2>Order Items</h2>
        <table className="receipt-items">
          <thead>
            <tr>
              <th>Item</th>
              <th>Qty</th>
              <th>Price</th>
              <th>Total</th>
            </tr>
          </thead>
          <tbody>
            {order.items.map((item) => (
              <tr key={item.productId}>
                <td>{item.name}</td>
                <td>{item.quantity}</td>
                <td>${item.unitPrice.toFixed(2)}</td>
                <td>${item.total.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className="receipt-total">
          <p><strong>Subtotal:</strong> ${order.total.toFixed(2)}</p>
          <p className={order.status === 'paid' ? 'status-paid' : ''}>
            {order.status}
          </p>
        </div>
      </div>
    </div>
  );
};