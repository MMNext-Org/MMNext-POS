import { useState } from 'react';
import { Button } from './ui/Button';
import { useShiftContext } from '@/context/ShiftContext';

interface ShiftHandoverProps {
  onClose: () => void;
}

export const ShiftHandover: React.FC<ShiftHandoverProps> = ({ onClose }) => {
  const { shift, staff, requestHandover } = useShiftContext();
  const [toUserId, setToUserId] = useState<string>('');
  const [error, setError] = useState<string | null>(null);

  if (!shift) return null;

  const eligible = staff.filter((s) => s.id !== shift.registrarId);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!toUserId) {
      setError('Select a colleague to hand the shift to.');
      return;
    }
    const target = eligible.find((s) => s.id === toUserId);
    requestHandover(toUserId, target ? target.name : '');
    onClose();
  };

  const target = eligible.find((s) => s.id === toUserId);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black bg-opacity-60 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-white w-full max-w-md p-6 rounded-lg shadow-xl">
        <h2 className="text-lg font-bold mb-4">Shift Handover</h2>
        <p className="text-sm text-gray-600 mb-6">
          Hand over the open shift <span className="font-mono">{shift.id.slice(0, 8)}</span> to another staff member.
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">Hand over to</label>
            <select
              value={toUserId}
              onChange={(e) => setToUserId(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
            >
              <option value="">-- Select staff --</option>
              {eligible.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name} ({s.role})
                </option>
              ))}
            </select>
          </div>
          {target && (
            <p className="text-xs text-gray-500">
              Current shift holder: {shift.registrarName}. Cash float: ${shift.total.toFixed(2)}.
            </p>
          )}
          {error && (
            <p className="text-xs text-red-600">{error}</p>
          )}
          <div className="flex gap-3 pt-4">
            <Button variant="ghost" onClick={onClose}>
              Cancel
            </Button>
            <Button variant="primary" type="submit" disabled={!toUserId}>
              Hand Over Shift
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
