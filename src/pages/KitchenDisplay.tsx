import { memo, useEffect } from 'react';
import { usePowerSyncStatus } from '../context/PowerSyncStatusContext';
import { useTenant } from '../context/TenantContext';
import { useOrderContext } from '@/context/OrderContext';

export // Status-to-class mapping hoisted out of the render path so it is not
// recreated on every render.
const orderStatusClasses = {
  pending: 'status-pending',
  processing: 'status-processing',
  completed: 'status-completed',
  paid: 'status-paid',
  voided: 'status-voided',
} as const;

interface OrderCardProps {
  order: {
    id: string;
    table?: string | null;
    status: string;
    createdAt: string;
    items: { productId: string; name: string; quantity: number; total: number }[];
  };
}

// Memoized order card: only re-renders when its own order prop changes,
// so date formatting and item rendering are bounded per order.
const OrderCard = memo(({ order }: OrderCardProps) => {
  const time = new Date(order.createdAt).toLocaleTimeString();
  const statusClass = orderStatusClasses[order.status] ?? '';

  return (
    <div key={order.id} className={`order-card status-${statusClass}`}>
      <div className="order-info">
        <h3>Table {order.table || '?'} - Order #{order.id}</h3>
        <p><strong>Status:</strong> {order.status}</p>
        <p><strong>Time:</strong> {time}</p>
      </div>

      <div className="order-items">
        {order.items.map((item) => (
          <div key={item.productId} className="menu-item">
            {item.quantity}x {item.name}
            <span className="price">{item.total.toFixed(2)}</span>
          </div>
        ))}
      </div>

      <div className="order-actions">
        <button className="action-btn" onClick={() => { /* Handle order actions */ }}>
          Update Status
        </button>
        <button className="action-btn secondary" disabled>
          Print Receipt
        </button>
      </div>
    </div>
  );
});

export const KitchenDisplay = () => {
  const { status } = usePowerSyncStatus();
  const { selectedBranch } = useTenant();
  const { orders } = useOrderContext();

  // Filter orders for this branch/kitchen
  const filteredOrders = orders.filter((order) =>
    order.branchId === selectedBranch?.id
  );

  useEffect(() => {
    // This would ideally listen to real-time order updates
    // For now, we'll just re-render when orders change
  }, [orders, selectedBranch]);

  return (
    <div className="kitchen-display" data-testid="kitchen-display">
      <div className="kitchen-header">
        <h1>Kitchen Display</h1>
        <div className="connection-status" role="status" aria-live="polite">
          <span className={`status-${status}`}>
            {status === 'connected' ? 'Ready' :
             status === 'disconnected' ? 'Offline' :
             status === 'error' ? 'Error' :
             status === 'syncing' ? 'Syncing' : 'Connecting'}
          </span>
        </div>
      </div>

      <div className="kitchen-orders">
        {filteredOrders.length === 0 ? (
          <p className="empty-state">No orders for this branch</p>
        ) : (
          <div className="order-list">
            {filteredOrders.map((order) => (
              <OrderCard key={order.id} order={order} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};