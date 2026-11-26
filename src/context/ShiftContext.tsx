import { createContext, useContext, useState, useCallback } from 'react';

export type ShiftStatus = 'open' | 'pending_approval' | 'closed' | 'handed_over';
export type ApprovalStatus = 'none' | 'pending' | 'approved' | 'rejected';
export type HandoverStatus = 'pending' | 'accepted' | 'rejected';
export type TipsStatus = 'pending' | 'completed' | 'reversed';

export interface ShiftUser {
  id: string;
  name: string;
  role: string;
}

export interface RegisterShift {
  id: string;
  organizationId: string;
  registrarId: string;
  registrarName: string;
  startTime: string;
  endTime: string | null;
  status: ShiftStatus;
  cashIn: number;
  cashOut: number;
  total: number;
  handedOverTo: string | null;
  handedOverBy: string | null;
  handedOverAt: string | null;
  approvalStatus: ApprovalStatus;
  approvedBy: string | null;
  approvedAt: string | null;
  tipsTotal: number;
  tipsAllocated: number;
  createdAt: string;
  updatedAt: string;
}

export interface ShiftHandover {
  id: string;
  organizationId: string;
  registerShiftId: string;
  fromUserId: string;
  toUserId: string;
  status: HandoverStatus;
  createdAt: string;
  updatedAt: string;
}

export interface TipsAllocationItem {
  userId: string;
  userName: string;
  amount: number;
}

export interface TipsAllocation {
  id: string;
  organizationId: string;
  registerShiftId: string;
  allocatedBy: string;
  totalAmount: number;
  status: TipsStatus;
  items: TipsAllocationItem[];
  createdAt: string;
  updatedAt: string;
}

export interface ShiftApproval {
  id: string;
  organizationId: string;
  registerShiftId: string;
  approverId: string;
  approverName: string;
  status: ApprovalStatus;
  reason: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CashTransaction {
  type: 'cash_in' | 'cash_out';
  amount: number;
  timestamp: string;
}

export interface ShiftContextType {
  // Current shift
  shift: RegisterShift | null;
  shifts: RegisterShift[];
  staff: ShiftUser[];
  loading: boolean;

  // Cash drawer (current shift)
  cashIn: (amount: number) => void;
  cashOut: (amount: number) => void;

  // Shift lifecycle
  openShift: (registrarId: string, registrarName: string, openingFloat: number) => void;
  requestClose: () => void; // shift -> pending_approval, creates approval request
  closeShift: (approverId: string, approverName: string) => void; // finalize after approval

  // Handover
  requestHandover: (toUserId: string, toUserName: string) => void;
  acceptHandover: (handoverId: string) => void;
  rejectHandover: (handoverId: string) => void;

  // Manager approval
  respondToApproval: (approve: boolean, reason?: string) => void;

  // Tips
  allocateTips: (totalAmount: number, items: TipsAllocationItem[]) => void;
  reverseTips: (allocationId: string) => void;
}

const now = () => new Date().toISOString();
const id = () => crypto.randomUUID();

const defaultStaff: ShiftUser[] = [
  { id: 'staff-1', name: 'Alex Rivera', role: 'Cashier' },
  { id: 'staff-2', name: 'Samir Patel', role: 'Cashier' },
  { id: 'staff-3', name: 'Jordan Lee', role: 'Shift Lead' },
  { id: 'staff-4', name: 'Casey Moore', role: 'Branch Manager' },
];

const ShiftContext = createContext<ShiftContextType | undefined>(undefined);
export const ShiftProvider = ({
  children,
  organizationId = 'org-1',
}: { children: React.ReactNode; organizationId?: string }) => {
  const [shift, setShift] = useState<RegisterShift | null>(null);
  const [shifts, setShifts] = useState<RegisterShift[]>([]);
  const [staff] = useState<ShiftUser[]>(defaultStaff);
  const [loading] = useState<boolean>(false);

  const cashIn = useCallback((amount: number) => {
    if (!shift) return;
    if (amount <= 0) return;
    const next: RegisterShift = {
      ...shift,
      cashIn: shift.cashIn + amount,
      total: shift.cashIn + amount - shift.cashOut,
      updatedAt: now(),
    };
    setShift(next);
    setShifts((prev) => prev.map((s) => (s.id === next.id ? next : s)));
  }, [shift]);

  const cashOut = useCallback((amount: number) => {
    if (!shift) return;
    if (amount <= 0) return;
    if (amount > shift.cashIn - shift.cashOut) return;
    const next: RegisterShift = {
      ...shift,
      cashOut: shift.cashOut + amount,
      total: shift.cashIn - (shift.cashOut + amount),
      updatedAt: now(),
    };
    setShift(next);
    setShifts((prev) => prev.map((s) => (s.id === next.id ? next : s)));
  }, [shift]);

  const openShift = useCallback(
    (registrarId: string, registrarName: string, openingFloat: number) => {
      const newShift: RegisterShift = {
        id: id(),
        organizationId,
        registrarId,
        registrarName,
        startTime: now(),
        endTime: null,
        status: 'open',
        cashIn: Math.max(0, openingFloat),
        cashOut: 0,
        total: Math.max(0, openingFloat),
        handedOverTo: null,
        handedOverBy: null,
        handedOverAt: null,
        approvalStatus: 'none',
        approvedBy: null,
        approvedAt: null,
        tipsTotal: 0,
        tipsAllocated: 0,
        createdAt: now(),
        updatedAt: now(),
      };
      setShift(newShift);
      setShifts((prev) => [newShift, ...prev]);
    },
    [organizationId]
  );

  const requestClose = useCallback(() => {
    if (!shift || shift.status !== 'open') return;
    const next: RegisterShift = { ...shift, status: 'pending_approval', updatedAt: now() };
    setShift(next);
    setShifts((prev) => prev.map((s) => (s.id === next.id ? next : s)));
  }, [shift]);

  const closeShift = useCallback(
    (approverId: string, _approverName: string) => {
      if (!shift || shift.approvalStatus !== 'approved') return;
      const next: RegisterShift = {
        ...shift,
        status: 'closed',
        endTime: now(),
        approvedBy: approverId,
        approvedAt: now(),
        updatedAt: now(),
      };
      setShift(next);
      setShifts((prev) => prev.map((s) => (s.id === next.id ? next : s)));
    },
    [shift]
  );

  const requestHandover = useCallback(
    (toUserId: string, _toUserName: string) => {
      if (!shift || shift.status !== 'open') return;
      const next: RegisterShift = {
        ...shift,
        status: 'handed_over',
        handedOverTo: toUserId,
        handedOverBy: shift.registrarId,
        handedOverAt: now(),
        updatedAt: now(),
      };
      setShift(next);
      setShifts((prev) => prev.map((s) => (s.id === next.id ? next : s)));
    },
    [shift]
  );

  const acceptHandover = useCallback((handoverId: string) => {
    // Handover acceptance is recorded; the shift status already reflects handed_over.
    // In a full implementation this would persist the handover record transition.
    console.log('Handover accepted:', handoverId);
  }, []);

  const rejectHandover = useCallback((handoverId: string) => {
    console.log('Handover rejected:', handoverId);
  }, []);

  const respondToApproval = useCallback(
    (approve: boolean, _reason?: string) => {
      if (!shift || shift.status !== 'pending_approval') return;
      const approvalStatus: ApprovalStatus = approve ? 'approved' : 'rejected';
      const next: RegisterShift = {
        ...shift,
        approvalStatus,
        status: approve ? 'pending_approval' : 'open', // approved remains pending_approval until closeShift; rejected reopens
        updatedAt: now(),
      };
      setShift(next);
      setShifts((prev) => prev.map((s) => (s.id === next.id ? next : s)));
    },
    [shift]
  );

  const allocateTips = useCallback(
    (totalAmount: number, items: TipsAllocationItem[]) => {
      if (!shift || shift.status !== 'closed') return;
      if (totalAmount <= 0) return;
      const allocated = items.reduce((sum, it) => sum + it.amount, 0);
      if (allocated !== totalAmount) return;

      const next: RegisterShift = {
        ...shift,
        tipsTotal: totalAmount,
        tipsAllocated: totalAmount,
        updatedAt: now(),
      };
      setShift(next);
      setShifts((prev) => prev.map((s) => (s.id === next.id ? next : s)));
    },
    [shift]
  );

  const reverseTips = useCallback((_allocationId: string) => {
    if (!shift) return;
    const next: RegisterShift = { ...shift, tipsTotal: 0, tipsAllocated: 0, updatedAt: now() };
    setShift(next);
    setShifts((prev) => prev.map((s) => (s.id === next.id ? next : s)));
  }, [shift]);

  const value: ShiftContextType = {
    shift,
    shifts,
    staff,
    loading,
    cashIn,
    cashOut,
    openShift,
    requestClose,
    closeShift,
    requestHandover,
    acceptHandover,
    rejectHandover,
    respondToApproval,
    allocateTips,
    reverseTips,
  };

  return (
    <ShiftContext.Provider value={value}>{children}</ShiftContext.Provider>
  );
};

export const useShiftContext = () => {
  const context = useContext(ShiftContext);
  if (!context) {
    throw new Error('useShiftContext must be used within ShiftProvider');
  }
  return context;
};

export default ShiftContext;
