import { useOrderContext } from '@/context/OrderContext';
import { QueueItem } from '@/context/OrderContext';

export const SyncQueue = () => {
  const { queueItems, error } = useOrderContext();

  if (queueItems.length === 0 && !error) {
    return <div>Sync queue is empty.</div>;
  }

  return (
    <div style={{ border: '1px solid #ccc', padding: '1rem', margin: '1rem 0' }}>
      <h3>Sync Queue</h3>
      {error && <p style={{ color: 'red' }}>Error: {error}</p>}
      <ul>
        {queueItems.map((item) => (
          <li key={item.id} style={{ marginBottom: '0.5rem' }}>
            <strong>{item.order.id}</strong> - Status: {item.status} &nbsp;|&nbsp;
            <em>{new Date(item.createdAt).toLocaleString()}</em>
          </li>
        ))}
      </ul>
    </div>
  );
};