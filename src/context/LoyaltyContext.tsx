import { createContext, useContext, useCallback, useState, useEffect } from 'react';
import { supabase } from '@/utils/supabase';
import { useAuth } from '@/context/AuthContext';

export type LoyaltyTransactionType = 'earn' | 'redeem' | 'expire' | 'adjust';

export interface LoyaltyCustomer {
  id: string;
  organizationId: string;
  name: string;
  email: string | null;
  phone: string | null;
  loyaltyPoints: number;
  createdAt: string;
  updatedAt: string;
}

export interface LoyaltyAccount {
  id: string;
  customerId: string;
  organizationId: string;
  pointsBalance: number;
  tier: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface LoyaltyTransaction {
  id: string;
  loyaltyAccountId: string;
  organizationId: string;
  customerId: string;
  type: LoyaltyTransactionType;
  points: number;
  balanceAfter: number;
  reason: string | null;
  referenceId: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface LoyaltyContextType {
  customers: LoyaltyCustomer[];
  accounts: LoyaltyAccount[];
  transactions: LoyaltyTransaction[];
  loading: boolean;
  error: string | null;

  // Customer management
  enrollCustomer: (name: string, email?: string, phone?: string) => Promise<LoyaltyCustomer | null>;
  findCustomer: (customerId: string) => LoyaltyCustomer | undefined;
  searchCustomers: (query: string) => LoyaltyCustomer[];

  // Loyalty account
  getAccount: (customerId: string) => LoyaltyAccount | undefined;
  disenrollCustomer: (customerId: string) => Promise<boolean>;

  // Points operations
  earnPoints: (customerId: string, points: number, reason?: string, referenceId?: string) => Promise<boolean>;
  redeemPoints: (customerId: string, points: number, reason?: string, referenceId?: string) => Promise<boolean>;

  // Transaction history
  getCustomerTransactions: (customerId: string) => LoyaltyTransaction[];

  // Refresh
  refresh: () => Promise<void>;
}

const LoyaltyContext = createContext<LoyaltyContextType | undefined>(undefined);

export const LoyaltyProvider = ({
  children,
}: { children: React.ReactNode }) => {
  const { user } = useAuth();
  const organizationId = user?.app_metadata?.organization_id ?? '';
  const userId = user?.id ?? '';

  const [customers, setCustomers] = useState<LoyaltyCustomer[]>([]);
  const [accounts, setAccounts] = useState<LoyaltyAccount[]>([]);
  const [transactions, setTransactions] = useState<LoyaltyTransaction[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const findCustomer = useCallback((customerId: string) => {
    return customers.find((c) => c.id === customerId);
  }, [customers]);

  const searchCustomers = useCallback((query: string) => {
    const q = query.toLowerCase();
    return customers.filter(
      (c) =>
        c.name.toLowerCase().includes(q) ||
        c.email?.toLowerCase().includes(q) ||
        c.phone?.toLowerCase().includes(q) ||
        c.id.includes(q),
    );
  }, [customers]);

  const getAccount = useCallback((customerId: string) => {
    return accounts.find((a) => a.customerId === customerId);
  }, [accounts]);

  const getCustomerTransactions = useCallback((customerId: string) => {
    return transactions.filter((t) => t.customerId === customerId);
  }, [transactions]);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [cRes, aRes, tRes] = await Promise.all([
        supabase.from('customers').select('*').eq('organization_id', organizationId),
        supabase.from('loyalty_accounts').select('*').eq('organization_id', organizationId),
        supabase.from('loyalty_transactions').select('*').eq('organization_id', organizationId).order('created_at', { ascending: false }),
      ]);
      if (cRes.error) throw cRes.error;
      if (aRes.error) throw aRes.error;
      if (tRes.error) throw tRes.error;

      setCustomers((cRes.data || []) as LoyaltyCustomer[]);
      setAccounts((aRes.data || []) as LoyaltyAccount[]);
      setTransactions((tRes.data || []) as LoyaltyTransaction[]);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to refresh loyalty data');
    } finally {
      setLoading(false);
    }
  }, [organizationId]);

  useEffect(() => {
    refresh();
  }, [refresh]);

  const enrollCustomer = useCallback(
    async (name: string, email?: string, phone?: string): Promise<LoyaltyCustomer | null> => {
      if (!organizationId) return null;
      if (!name || name.trim().length === 0) return null;

      setLoading(true);
      setError(null);
      try {
        // Insert customer record first (RPC requires customer to exist)
        const customerId = crypto.randomUUID();
        const { error: cError } = await supabase.from('customers').insert({
          id: customerId,
          organization_id: organizationId,
          name: name.trim(),
          email: email || null,
          phone: phone || null,
          consent: false,
          created_at: new Date().toISOString(),
          updated_at: new Date().toISOString(),
        });
        if (cError) throw cError;

        // Call enroll RPC to create loyalty account
        const { data, error: rpcError } = await supabase.rpc('rpc_loyalty_enroll', {
          p_user_id: userId,
          p_customer_id: customerId,
          p_org_id: organizationId,
          p_consent_version: '1.0',
        });
        if (rpcError) throw rpcError;

        const account = (data || [])[0];
        if (!account) return null;

        const newCustomer: LoyaltyCustomer = {
          id: customerId,
          organizationId,
          name: name.trim(),
          email: email || null,
          phone: phone || null,
          loyaltyPoints: account.points_balance ?? 0,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };

        const newAccount: LoyaltyAccount = {
          id: account.loyalty_account_id,
          customerId,
          organizationId,
          pointsBalance: account.points_balance ?? 0,
          tier: account.status === 'active' ? 'bronze' : null,
          createdAt: account.enrolled_at ?? new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };

        setCustomers((prev) => [...prev, newCustomer]);
        setAccounts((prev) => [...prev, newAccount]);

        return newCustomer;
      } catch (err) {
        const msg = err instanceof Error ? err.message : 'Failed to enroll customer';
        setError(msg);
        return null;
      } finally {
        setLoading(false);
      }
    },
    [organizationId],
  );

  const disenrollCustomer = useCallback(
    async (customerId: string): Promise<boolean> => {
      if (!organizationId) return false;
      setLoading(true);
      setError(null);
      try {
        const { error: rpcError } = await supabase.rpc('rpc_loyalty_disenroll', {
          p_user_id: userId,
          p_customer_id: customerId,
          p_org_id: organizationId,
        });
        if (rpcError) throw rpcError;

        setTransactions((prev) => prev.filter((t) => t.customerId !== customerId));
        setAccounts((prev) => prev.filter((a) => a.customerId !== customerId));
        setCustomers((prev) => prev.filter((c) => c.id !== customerId));

        return true;
      } catch (err) {
        const msg = err instanceof Error ? err.message : 'Failed to disenroll customer';
        setError(msg);
        return false;
      } finally {
        setLoading(false);
      }
    },
    [organizationId],
  );

  const earnPoints = useCallback(
    async (customerId: string, points: number, reason?: string, referenceId?: string): Promise<boolean> => {
      if (!organizationId) return false;
      if (!points || points <= 0) return false;

      const account = getAccount(customerId);
      if (!account) return false;

      setLoading(true);
      setError(null);
      try {
        const newBalance = account.pointsBalance + points;
        const txnId = crypto.randomUUID();

        // Insert transaction
        const { error: tError } = await supabase.from('loyalty_transactions').insert({
          id: txnId,
          loyalty_account_id: account.id,
          organization_id: organizationId,
          customer_id: customerId,
          type: 'earn',
          points,
          balance_after: newBalance,
          reason: reason || null,
          reference_id: referenceId || null,
          created_at: new Date().toISOString(),
          updated_at: new Date().toISOString(),
        });
        if (tError) throw tError;

        // Update account balance
        const { error: aError } = await supabase
          .from('loyalty_accounts')
          .update({ points_balance: newBalance, updated_at: new Date().toISOString() })
          .eq('id', account.id);
        if (aError) throw aError;

        const txn: LoyaltyTransaction = {
          id: txnId,
          loyaltyAccountId: account.id,
          organizationId,
          customerId,
          type: 'earn',
          points,
          balanceAfter: newBalance,
          reason: reason || null,
          referenceId: referenceId || null,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };

        setTransactions((prev) => [txn, ...prev]);
        setAccounts((prev) =>
          prev.map((a) => (a.id === account.id ? { ...a, pointsBalance: newBalance, updatedAt: new Date().toISOString() } : a)),
        );
        setCustomers((prev) =>
          prev.map((c) => (c.id === customerId ? { ...c, loyaltyPoints: newBalance, updatedAt: new Date().toISOString() } : c)),
        );

        return true;
      } catch (err) {
        const msg = err instanceof Error ? err.message : 'Failed to earn points';
        setError(msg);
        return false;
      } finally {
        setLoading(false);
      }
    },
    [accounts, customers, organizationId, getAccount],
  );

  const redeemPoints = useCallback(
    async (customerId: string, points: number, reason?: string, referenceId?: string): Promise<boolean> => {
      if (!organizationId) return false;
      if (!points || points <= 0) return false;

      const account = getAccount(customerId);
      if (!account) return false;
      if (account.pointsBalance < points) return false;

      setLoading(true);
      setError(null);
      try {
        const rewardId = crypto.randomUUID();
        const idempotencyKey = crypto.randomUUID();

        // Step 1: Reserve points via RPC
        const { data: reserveData, error: reserveError } = await supabase.rpc('rpc_loyalty_redeem_reserve', {
          p_user_id: userId,
          p_customer_id: customerId,
          p_org_id: organizationId,
          p_points: points,
          p_reward_id: rewardId,
          p_branch_id: null,
          p_idempotency_key: idempotencyKey,
        });
        if (reserveError) throw reserveError;

        const reservation = (reserveData || [])[0];
        if (!reservation) return false;

        // Step 2: Commit the reservation
        const { error: commitError } = await supabase.rpc('rpc_loyalty_redeem_commit', {
          p_reservation_id: reservation.reservation_id,
          p_user_id: userId,
        });
        if (commitError) throw commitError;

        const newBalance = account.pointsBalance - points;
        const txnId = crypto.randomUUID();

        const txn: LoyaltyTransaction = {
          id: txnId,
          loyaltyAccountId: account.id,
          organizationId,
          customerId,
          type: 'redeem',
          points: -points,
          balanceAfter: newBalance,
          reason: reason || null,
          referenceId: referenceId || null,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        };

        setTransactions((prev) => [txn, ...prev]);
        setAccounts((prev) =>
          prev.map((a) => (a.id === account.id ? { ...a, pointsBalance: newBalance, updatedAt: new Date().toISOString() } : a)),
        );
        setCustomers((prev) =>
          prev.map((c) => (c.id === customerId ? { ...c, loyaltyPoints: newBalance, updatedAt: new Date().toISOString() } : c)),
        );

        return true;
      } catch (err) {
        const msg = err instanceof Error ? err.message : 'Failed to redeem points';
        setError(msg);
        return false;
      } finally {
        setLoading(false);
      }
    },
    [accounts, customers, organizationId, getAccount],
  );

  const value: LoyaltyContextType = {
    customers,
    accounts,
    transactions,
    loading,
    error,
    findCustomer,
    searchCustomers,
    enrollCustomer,
    getAccount,
    disenrollCustomer,
    earnPoints,
    redeemPoints,
    getCustomerTransactions,
    refresh,
  };

  return (
    <LoyaltyContext.Provider value={value}>{children}</LoyaltyContext.Provider>
  );
};

export const useLoyaltyContext = () => {
  const context = useContext(LoyaltyContext);
  if (!context) {
    throw new Error('useLoyaltyContext must be used within LoyaltyProvider');
  }
  return context;
};

export default LoyaltyContext;
