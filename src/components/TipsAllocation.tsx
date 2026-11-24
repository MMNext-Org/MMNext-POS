import { useState } from 'react';
import { Button } from './ui/Button';
import { useShiftContext } from '@/context/ShiftContext';

interface TipsAllocationProps {
  onClose: () => void;
}

export const TipsAllocation: React.FC<TipsAllocationProps> = ({ onClose }) => {
  const { shift, staff, allocateTips } = useShiftContext();
  const [totalAmount, setTotalAmount] = useState('');
  const [items, setItems] = useState<{ userId: string; amount: string }[]>([]);
  const [error, setError] = useState<string | null>(null);

  if (!shift) return null;

  const parsedTotal = parseFloat(totalAmount) || 0;
  const allocatedSum = items.reduce((sum, it) => sum + (parseFloat(it.amount) || 0), 0);

  const addItem = () => {
    setItems([...items, { userId: '', amount: '' }]);
  };

  const removeItem = (index: number) => {
    setItems(items.filter((_, i) => i !== index));
  };

  const handleChangeUser = (index: number, value: string) => {
    const next = [...items];
    next[index].userId = value;
    setItems(next);
  };

  const handleChangeAmount = (index: number, value: string) => {
    const next = [...items];
    next[index].amount = value;
    setItems(next);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (parsedTotal <= 0) {
      setError('Enter a valid total tip amount.');
      return;
    }
    if (items.length === 0) {
      setError('Add at least one recipient.');
      return;
    }
    if (Math.abs(allocatedSum - parsedTotal) > 0.009) {
      setError('Item amounts must add up to the total.');
      return;
    }
    const allocationItems = items.map((it) => {
      const user = staff.find((s) => s.id === it.userId);
      return { userId: it.userId, userName: user?.name || 'Unknown', amount: parseFloat(it.amount) || 0 };
    });
    allocateTips(parsedTotal, allocationItems);
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black bg-opacity-60 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-white w-full max-w-md p-6 rounded-lg shadow-xl">
        <h2 className="text-lg font-bold mb-4">Tips Allocation</h2>
        <p className="text-sm text-gray-600 mb-6">
          Distribute the tip pool for the closed shift <span className="font-mono">{shift.id.slice(0, 8)}</span>.
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">Total tip amount ($)</label>
            <input
              type="number"
              step="0.01"
              min="0"
              value={totalAmount}
              onChange={(e) => setTotalAmount(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
            />
          </div>

          <div>
            <div className="flex justify-between items-center mb-2">
              <label className="text-sm font-medium">Allocate to staff</label>
              <Button variant="secondary" size="sm" type="button" onClick={addItem}>
                + Add recipient
              </Button>
            </div>
            {items.map((it, index) => {
              const user = staff.find((s) => s.id === it.userId);
              return (
                <div key={index} className="flex gap-2 items-center">
                  <select
                    value={it.userId}
                    onChange={(e) => handleChangeUser(index, e.target.value)}
                    className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                  >
                    <option value="">-- Select staff --</option>
                    {staff.map((s) => (
                      <option key={s.id} value={s.id}>{s.name} ({s.role})</option>
                    ))}
                  </select>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    value={it.amount}
                    onChange={(e) => handleChangeAmount(index, e.target.value)}
                    placeholder="$0.00"
                    className="w-24 px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                  />
                  <Button variant="ghost" size="sm" type="button" onClick={() => removeItem(index)}>
                    ×
                  </Button>
                </div>
              );
            })}
            {items.length === 0 && (
              <p className="text-xs text-gray-500 italic">No recipients added yet.</p>
            )}
          </div>

          <p className="text-sm">
            Allocated: ${allocatedSum.toFixed(2)} / Total: ${parsedTotal.toFixed(2)}
          </p>
          {error && <p className="text-xs text-red-600">{error}</p>}
          <div className="flex gap-3 pt-4">
            <Button variant="ghost" onClick={onClose}>
              Cancel
            </Button>
            <Button
              variant="primary"
              type="submit"
              disabled={parsedTotal <= 0 || items.length === 0 || Math.abs(allocatedSum - parsedTotal) > 0.009}
            >
              Allocate Tips
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
