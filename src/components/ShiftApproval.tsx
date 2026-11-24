import { useState } from 'react';
import { Button } from './ui/Button';
import { useShiftContext } from '@/context/ShiftContext';

interface ShiftApprovalProps {
  shift: { id: string; registrarName: string; status: string; cashIn: number; cashOut: number; total: number; tipsTotal: number; tipsAllocated: number };
  onClose: () => void;
}

export const ShiftApproval: React.FC<ShiftApprovalProps> = ({ shift, onClose }) => {
  const { staff, respondToApproval, closeShift } = useShiftContext();
  const [reason, setReason] = useState('');

  const canApprove = shift.status === 'pending_approval';
  const approver = staff.find((s) => s.role === 'Branch Manager');

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black bg-opacity-60 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-white w-full max-w-md p-6 rounded-lg shadow-xl">
        <h2 className="text-lg font-bold mb-4">Manager Approval — Shift Close</h2>
        <p className="text-sm text-gray-600 mb-6">
          Shift <span className="font-mono">{shift.id.slice(0, 8)}</span> held by{' '}
          <span className="font-medium">{shift.registrarName}</span> is awaiting manager approval to close.
        </p>

        <div className="space-y-3 mb-6">
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Cash in</span>
            <span className="font-mono">${shift.cashIn.toFixed(2)}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Cash out</span>
            <span className="font-mono">${shift.cashOut.toFixed(2)}</span>
          </div>
          <div className="flex justify-between text-sm font-bold">
            <span className="text-gray-900">Net total</span>
            <span className="font-mono">${shift.total.toFixed(2)}</span>
          </div>
          {shift.tipsTotal > 0 && (
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Tips allocated</span>
              <span className="font-mono">${shift.tipsAllocated.toFixed(2)}</span>
            </div>
          )}
        </div>

        {canApprove && (
          <>
            <div>
              <label className="block text-sm font-medium mb-1">Approval reason / notes</label>
              <textarea
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
              />
            </div>
            <div className="flex gap-3 pt-6">
              <Button variant="ghost" onClick={onClose}>
                Cancel
              </Button>
              <Button variant="danger" type="button" onClick={() => respondToApproval(false, reason || undefined)}>
                Reject
              </Button>
              <Button variant="primary" type="button" onClick={() => {
                respondToApproval(true, reason || undefined);
                closeShift('staff-4', 'Casey Moore');
              }}>
                Approve &amp; Close
              </Button>
            </div>
          </>
        )}

        {!canApprove && (
          <div className="text-sm text-gray-500 text-center pt-6">
            This shift cannot be approved at this time (status: {shift.status}).
          </div>
        )}
        {!approver && canApprove && (
          <p className="text-xs text-gray-400 text-center mt-2">
            No branch manager found in staff list.
          </p>
        )}
      </div>
    </div>
  );
};
