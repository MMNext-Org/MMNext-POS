import { useState, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useShiftContext } from '@/context/ShiftContext';
import { Button } from '@/components/ui/Button';
import { ShiftHandover } from '@/components/ShiftHandover';
import { TipsAllocation } from '@/components/TipsAllocation';
import { ShiftApproval } from '@/components/ShiftApproval';

export const Shifts = () => {
  const {
    shift,
    shifts,
    staff,
    cashIn,
    cashOut,
    openShift,
    requestClose,
    closeShift,
  } = useShiftContext();
  const [openHandover, setOpenHandover] = useState(false);
  const [openTips, setOpenTips] = useState(false);
  const [openApproval, setOpenApproval] = useState(false);
  const [staffId, setStaffId] = useState('staff-1');
  const [openingFloat, setOpeningFloat] = useState('0');
  const cashAmountRef = useRef<HTMLInputElement>(null);

  const staffMember = staff.find((s) => s.id === staffId);

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-4xl mx-auto">
        <div className="flex justify-between items-end mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Shift Management</h1>
            <p className="text-gray-600">
              Open, close, hand over, and allocate tips for register shifts.
            </p>
          </div>
          <Link to="/" className="text-sm text-gray-600 hover:text-gray-900 underline">
            ← Back to Home
          </Link>
        </div>

        {!shift ? (
          <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
            <h2 className="text-lg font-bold mb-4">Open a Shift</h2>
            <p className="text-gray-600 mb-4">
              No active shift. Open a register shift to begin taking cash sales.
            </p>
            <div className="space-y-4">
              <label className="block text-sm font-medium">Staff member opening the shift</label>
              <select
                value={staffId}
                onChange={(e) => setStaffId(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
              >
                {staff.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name} ({s.role})
                  </option>
                ))}
              </select>
              <div className="flex gap-4 items-end">
                <div className="flex-1">
                  <label className="block text-sm font-medium mb-1">Opening float ($)</label>
                  <input
                    value={openingFloat}
                    onChange={(e) => setOpeningFloat(e.target.value)}
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="0.00"
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                  />
                </div>
                <Button
                  variant="primary"
                  onClick={() => openShift(staffId, staffMember?.name || 'Unknown', parseFloat(openingFloat) || 0)}
                >
                  Open Shift
                </Button>
              </div>
            </div>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
              <div className="bg-white p-5 rounded-lg shadow border border-gray-200">
                <p className="text-xs font-semibold text-gray-500 uppercase">Status</p>
                <p className={`text-lg font-medium ${shift.status === 'open' ? 'text-green-700' : 'text-gray-700'}`}>
                  {shift.status.replace(/_/g, ' ')}
                </p>
              </div>
              <div className="bg-white p-5 rounded-lg shadow border border-gray-200">
                <p className="text-xs font-semibold text-gray-500 uppercase">Cash in</p>
                <p className="text-lg font-medium">${shift.cashIn.toFixed(2)}</p>
              </div>
              <div className="bg-white p-5 rounded-lg shadow border border-gray-200">
                <p className="text-xs font-semibold text-gray-500 uppercase">Cash out</p>
                <p className="text-lg font-medium">${shift.cashOut.toFixed(2)}</p>
              </div>
              <div className="bg-white p-5 rounded-lg shadow border border-gray-200">
                <p className="text-xs font-semibold text-gray-500 uppercase">Net total</p>
                <p className="text-lg font-bold">${shift.total.toFixed(2)}</p>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
              <h2 className="text-lg font-bold mb-4">Cash Drawer</h2>
              <div className="flex gap-3 mb-6 items-end">
                <span className="text-lg font-medium text-gray-900">$</span>
                <input
                  ref={cashAmountRef}
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="0.00"
                  aria-label="Cash amount"
                  className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                />
                <Button variant="primary" size="sm" onClick={() => cashIn(parseFloat(cashAmountRef.current?.value || '0') || 0)} aria-label="Cash in">
                  Cash In
                </Button>
                <Button variant="secondary" size="sm" onClick={() => cashOut(parseFloat(cashAmountRef.current?.value || '0') || 0)} aria-label="Cash out">
                  Cash Out
                </Button>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                {shift.status === 'open' && (
                  <Button variant="danger" onClick={requestClose} className="w-full">
                    Request Shift Close (needs manager approval)
                  </Button>
                )}
                {shift.status === 'open' && (
                  <Button variant="secondary" onClick={() => setOpenHandover(true)} className="w-full">
                    Hand Over Shift
                  </Button>
                )}
                {shift.approvalStatus === 'approved' && shift.status === 'pending_approval' && (
                  <Button variant="primary" onClick={() => closeShift('staff-4', 'Casey Moore')} className="w-full">
                    Finalize Close (approved)
                  </Button>
                )}
                {shift.status === 'closed' && (
                  <Button variant="secondary" onClick={() => setOpenTips(true)} className="w-full">
                    Allocate Tips
                  </Button>
                )}
              </div>

              {shift.status === 'pending_approval' && shift.approvalStatus === 'none' && (
                <div className="bg-amber-50 border border-amber-200 rounded p-4 mb-6">
                  <p className="text-sm text-amber-800">
                    Shift close is pending manager approval. A manager must approve before the shift can be finalized.
                  </p>
                </div>
              )}

              {shift.status === 'pending_approval' && shift.approvalStatus === 'approved' && (
                <div className="bg-green-50 border border-green-200 rounded p-4 mb-6">
                  <p className="text-sm text-green-800">
                    Manager approved. Click "Finalize Close" to complete the shift.
                  </p>
                </div>
              )}

              {shift.status === 'pending_approval' && shift.approvalStatus === 'rejected' && (
                <div className="bg-red-50 border border-red-200 rounded p-4 mb-6">
                  <p className="text-sm text-red-800">
                    Close was rejected by a manager. The shift remains open.
                  </p>
                </div>
              )}

              {shift.status === 'handed_over' && (
                <div className="bg-blue-50 border border-blue-200 rounded p-4 mb-6">
                  <p className="text-sm text-blue-800">
                    This shift has been handed over to a colleague.
                  </p>
                </div>
              )}

              {shift.status === 'pending_approval' && (
                <Button variant="ghost" onClick={() => setOpenApproval(true)} className="w-full mb-6">
                  Review Manager Approval
                </Button>
              )}
            </div>
          </>
        )}

        {openHandover && <ShiftHandover onClose={() => setOpenHandover(false)} />}
        {openTips && <TipsAllocation onClose={() => setOpenTips(false)} />}
        {openApproval && shift && <ShiftApproval shift={shift} onClose={() => setOpenApproval(false)} />}

        {shifts.length > 1 && (
          <div className="mt-8 bg-white rounded-lg shadow border border-gray-200 p-6">
            <h2 className="text-lg font-bold mb-4">Recent Shifts</h2>
            <table className="w-full text-left text-sm">
              <thead className="bg-gray-100">
                <tr>
                  <th className="px-4 py-2 font-semibold text-gray-600">Shift</th>
                  <th className="px-4 py-2 font-semibold text-gray-600">Staff</th>
                  <th className="px-4 py-2 font-semibold text-gray-600">Status</th>
                  <th className="px-4 py-2 font-semibold text-gray-600 text-right">Total</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {shifts.slice(0, 5).map((s) => (
                  <tr key={s.id}>
                    <td className="px-4 py-3 font-mono">{s.id.slice(0, 8)}</td>
                    <td className="px-4 py-3">{s.registrarName}</td>
                    <td className="px-4 py-3">{s.status.replace(/_/g, ' ')}</td>
                    <td className="px-4 py-3 text-right font-mono">${s.total.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default Shifts;
