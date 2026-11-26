import { useOrderContext } from '@/context/OrderContext';
import { EmptyState } from '@/components/ui/EmptyState';

export const SyncQueue = () => {
  const { queueItems, error } = useOrderContext();

  if (queueItems.length === 0 && !error) {
    return <EmptyState icon="📭" title="Sync queue is empty" />;
  }

  return (
    <div className="border border-gray-300 p-4 m-4">
      <h3 className="font-semibold text-gray-900 mb-2">Sync Queue</h3>
      {error && <p className="text-red-600 text-sm mb-3">Error: {error}</p>}
      <ul>
        {queueItems.map((item) => (
          <li key={item.id} className="mb-2">
            <strong>{item.order.id}</strong> - Status: {item.status} &nbsp;|&nbsp;
            <em>{new Date(item.createdAt).toLocaleString()}</em>
          </li>
        ))}
      </ul>
    </div>
  );
};