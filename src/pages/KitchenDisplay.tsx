import { useEffect, useState } from 'react';
import { usePowerSyncStatus } from '../context/PowerSyncStatusContext';
import { useTenant } from '../context/TenantContext';
import { OrderProvider } from '@/context/OrderContext';
import { useOrders } from '../context/OrderContext';
import { ReceiptView } from './ReceiptView';

export const KitchenDisplay = () => {
  const { status } = usePowerSyncStatus();
  const { selectedOrganization, selectedBranch } = useTenant();
  const { orders } = useOrders();
  
  // Filter orders for this branch/kitchen
  const filteredOrders = orders.filter(order => 
    order.branchId === selectedBranch?.id
  );

  const [filterStatus, setFilterStatus] = useState('all');

  useEffect(() => {
    // This would ideally listen to real-time order updates
    // For now, we'll just re-render when orders change
  }, [orders, selectedBranch]);

  const orderStatusClasses = {
    'pending': 'status-pending',
    'processing': 'status-processing',
    'completed': 'status-completed',
    'paid': 'status-paid',
    'voided': 'status-voided'
  };

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
            {filteredOrders.map(order => (
              <div 
                key={order.id} 
                className={`order-card status-${orderStatusClasses[order.status]}`}
              >
                <div className="order-info">
                  <h3>Table {order.table || '?'} - Order #{order.id}</h3>
                  <p><strong>Status:</strong> {order.status}</p>
                  <p><strong>Time:</strong> {new Date(order.createdAt).toLocaleTimeString()}</p>
                </div>
                
                <div className="order-items">
                  {order.items.map((item, index) => (
                    <div 
                      key={item.productId} 
                      className="menu-item"
                    >
                      {item.quantity}x {item.name}
                      <span className="price">{item.total.toFixed(2)}</span>
                    </div>
                  ))}
                </div>
                
                <div className="order-actions">
                  <button 
                    className="action-btn"
                    onClick={() => {/* Handle order actions */}}
                  >
                    Update Status
                  </button>
                  <button 
                    className="action-btn secondary"
                    onClick={() => {/* Print receipt */}}
                    disabled={status === 'disconnected'}
                  >
                    Print Receipt
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};