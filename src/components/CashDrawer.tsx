import { useShiftContext } from '@/context/ShiftContext';
import { Button } from '@/components/ui/Button';

export const CashDrawer = () => {
  const { shift, cashIn, cashOut } = useShiftContext();

  if (!shift) return null;

  return (
    <div className="cash-drawer p-4 bg-white rounded-lg shadow border border-gray-200">
      <h3 className="font-semibold mb-2">Cash Drawer — {shift.status.replace(/_/g, ' ')}</h3>
      <p className="text-sm text-gray-600 mb-3">
        Cash in: ${shift.cashIn.toFixed(2)} &nbsp;|&nbsp; Cash out: ${shift.cashOut.toFixed(2)} &nbsp;|&nbsp; Net: ${shift.total.toFixed(2)}
      </p>
      <div className="flex gap-2">
        <Button
          onClick={() => {
            const amt = window.prompt('Cash in amount:');
            if (amt) cashIn(parseFloat(amt));
          }}
        >
          Cash In
        </Button>
        <Button
          variant="secondary"
          onClick={() => {
            const amt = window.prompt('Cash out amount:');
            if (amt) cashOut(parseFloat(amt));
          }}
        >
          Cash Out
        </Button>
      </div>
    </div>
  );
};