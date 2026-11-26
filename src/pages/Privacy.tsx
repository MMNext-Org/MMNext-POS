import { useMemo, useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useLoyaltyContext, type LoyaltyCustomer } from '@/context/LoyaltyContext';
import { useAuth } from '@/context/AuthContext';
import { supabase } from '@/utils/supabase';
import { Button } from '@/components/ui/Button';
import { useToast } from '@/components/ui/Toast';

type ConsentType = 'loyalty_enrollment' | 'marketing' | 'data_processing' | 'terms_of_service';
type DataType = 'customer_profile' | 'loyalty_transactions' | 'consent_history' | 'loyalty_accounts';

const EXPORT_VERSION = '1.0';

export const Privacy = () => {
  const { user } = useAuth();
  const organizationId = user?.app_metadata?.organization_id ?? '';
  const userId = user?.id ?? '';

  const {
    customers,
    accounts,
    transactions,
    disenrollCustomer,
  } = useLoyaltyContext();

  const [exportReady, setExportReady] = useState(false);
  const [exportJson, setExportJson] = useState('');
  const [deleteCustomerId, setDeleteCustomerId] = useState<string | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);

  // Consent history state
  const [consentHistory, setConsentHistory] = useState<Array<{
    id: string;
    customer_id: string;
    organization_id: string;
    consent_type: string;
    consented: boolean;
    consent_version: string;
    consented_at: string;
    created_at: string;
  }>>([]);
  const [consentLoading, setConsentLoading] = useState(false);
  const [recordingConsent, setRecordingConsent] = useState(false);
  const [newConsentCustomerId, setNewConsentCustomerId] = useState<string | null>(null);
  const [newConsentType, setNewConsentType] = useState<ConsentType>('loyalty_enrollment');
  const [newConsentGiven, setNewConsentGiven] = useState(true);
  const [newConsentVersion, setNewConsentVersion] = useState('1.0');

  // Retention policy state
  const [retentionPolicies, setRetentionPolicies] = useState<Array<{
    id: string;
    organization_id: string;
    data_type: string;
    retention_period_days: number;
    auto_delete: boolean;
    created_at: string;
    updated_at: string;
  }>>([]);
  const [retentionLoading, setRetentionLoading] = useState(false);
  const [settingPolicy, setSettingPolicy] = useState(false);
  const [newPolicyDataType, setNewPolicyDataType] = useState<DataType>('customer_profile');
  const [newPolicyDays, setNewPolicyDays] = useState('');
  const [newPolicyAutoDelete, setNewPolicyAutoDelete] = useState(false);
  const [applyingRetention, setApplyingRetention] = useState(false);
  const [expiredRecords, setExpiredRecords] = useState<Array<{
    data_type: string;
    record_id: string;
    days_past_retention: number;
    action: string;
  }>>([]);

  const { toast } = useToast();

  useEffect(() => {
    loadConsentHistory();
    loadRetentionPolicies();
  }, []);

  // O(1) customer lookup per consent row instead of O(n) find per render.
  const customersMap = useMemo(() => {
    const map = new Map<string, LoyaltyCustomer>();
    customers.forEach((c) => map.set(c.id, c));
    return map;
  }, [customers]);

  const loadConsentHistory = async () => {
    setConsentLoading(true);
    try {
      const { data, error } = await supabase
        .from('consent_history')
        .select('*')
        .order('created_at', { ascending: false });
      if (error) throw error;
      setConsentHistory((data || []) as any);
    } catch (err) {
      toast({ title: 'Failed to load consent history', description: err instanceof Error ? err.message : 'Unknown error', type: 'error' });
    } finally {
      setConsentLoading(false);
    }
  };

  const handleRecordConsent = async () => {
    if (!newConsentCustomerId) return;
    setRecordingConsent(true);
    try {
      const { error } = await supabase.rpc('rpc_record_consent', {
        p_user_id: userId,
        p_customer_id: newConsentCustomerId,
        p_org_id: organizationId,
        p_consent_type: newConsentType,
        p_consented: newConsentGiven,
        p_consent_version: newConsentVersion,
      });
      if (error) throw error;
      toast({ title: 'Consent recorded', description: 'Customer consent has been saved.', type: 'success' });
      setNewConsentCustomerId(null);
      setNewConsentType('loyalty_enrollment');
      setNewConsentGiven(true);
      setNewConsentVersion('1.0');
      await loadConsentHistory();
    } catch (err) {
      toast({ title: 'Failed to record consent', description: err instanceof Error ? err.message : 'Unknown error', type: 'error' });
    } finally {
      setRecordingConsent(false);
    }
  };

  const loadRetentionPolicies = async () => {
    setRetentionLoading(true);
    try {
      const { data, error } = await supabase
        .from('retention_policy')
        .select('*')
        .order('data_type', { ascending: true });
      if (error) throw error;
      setRetentionPolicies((data || []) as any);
    } catch (err) {
      toast({ title: 'Failed to load retention policies', description: err instanceof Error ? err.message : 'Unknown error', type: 'error' });
    } finally {
      setRetentionLoading(false);
    }
  };

  const handleSetRetentionPolicy = async () => {
    if (!newPolicyDays || parseInt(newPolicyDays, 10) <= 0) return;
    setSettingPolicy(true);
    try {
      const { error } = await supabase.rpc('rpc_set_retention_policy', {
        p_user_id: userId,
        p_org_id: organizationId,
        p_data_type: newPolicyDataType,
        p_retention_period_days: parseInt(newPolicyDays, 10),
        p_auto_delete: newPolicyAutoDelete,
      });
      if (error) throw error;
      toast({ title: 'Retention policy set', description: `${newPolicyDataType} retention updated to ${newPolicyDays} days.`, type: 'success' });
      setNewPolicyDays('');
      setNewPolicyAutoDelete(false);
      await loadRetentionPolicies();
    } catch (err) {
      toast({ title: 'Failed to set retention policy', description: err instanceof Error ? err.message : 'Unknown error', type: 'error' });
    } finally {
      setSettingPolicy(false);
    }
  };

  const handleApplyRetention = async () => {
    setApplyingRetention(true);
    try {
      const { data, error } = await supabase.rpc('rpc_apply_retention', {
        p_user_id: userId,
        p_org_id: organizationId,
        p_data_type: null,
      });
      if (error) throw error;
      const expired = (data || []) as any;
      setExpiredRecords(expired);
      toast({ title: 'Retention applied', description: `${expired.length} record(s) past retention found.`, type: 'success' });
    } catch (err) {
      toast({ title: 'Failed to apply retention', description: err instanceof Error ? err.message : 'Unknown error', type: 'error' });
    } finally {
      setApplyingRetention(false);
    }
  };

  const handleExport = () => {
    const customersWithData = customers.map((customer) => {
      const account = accounts.find((a) => a.customerId === customer.id);
      const customerTransactions = transactions.filter((t) => t.customerId === customer.id);
      const customerConsent = consentHistory.filter((c) => c.customer_id === customer.id);

      return {
        profile: {
          id: customer.id,
          name: customer.name,
          email: customer.email,
          phone: customer.phone,
          consent: customerConsent.length > 0,
          createdAt: customer.createdAt,
          updatedAt: customer.updatedAt,
        },
        loyaltyAccount: account
          ? {
              id: account.id,
              pointsBalance: account.pointsBalance,
              tier: account.tier,
              createdAt: account.createdAt,
              updatedAt: account.updatedAt,
            }
          : null,
        transactions: customerTransactions.map((t) => ({
          id: t.id,
          type: t.type,
          points: t.points,
          balanceAfter: t.balanceAfter,
          reason: t.reason,
          referenceId: t.referenceId,
          createdAt: t.createdAt,
          updatedAt: t.updatedAt,
        })),
        consentHistory: customerConsent.map((c) => ({
          id: c.id,
          consentType: c.consent_type,
          consented: c.consented,
          consentVersion: c.consent_version,
          consentedAt: c.consented_at,
          createdAt: c.created_at,
        })),
      };
    });

    const exportData = {
      version: EXPORT_VERSION,
      exportedAt: new Date().toISOString(),
      source: 'BREWFLOW POS - Privacy Data Export',
      organizationId,
      exportedBy: userId,
      customers: customersWithData,
      summary: {
        customerCount: customers.length,
        accountCount: accounts.length,
        transactionCount: transactions.length,
        consentRecordCount: consentHistory.length,
        totalPoints: customers.reduce((sum, c) => sum + c.loyaltyPoints, 0),
      },
    };
    const json = JSON.stringify(exportData, null, 2);
    setExportJson(json);
    setExportReady(true);
    toast({ title: 'Export ready', description: `${customers.length} customer(s) included. Click Download to save.`, type: 'success' });
  };

  const downloadExport = () => {
    if (!exportJson) return;
    const blob = new Blob([exportJson], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `brewflow-privacy-export-v${EXPORT_VERSION}-${new Date().toISOString().slice(0, 10)}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    toast({ title: 'Export downloaded', description: 'Your data export has been saved.', type: 'success' });
  };

  const handleDeleteConfirm = () => {
    if (!deleteCustomerId) return;
    setDeleteLoading(true);

    try {
      const ok = disenrollCustomer(deleteCustomerId);
      if (ok) {
        toast({ title: 'Customer deleted', description: `Customer ${deleteCustomerId} and all associated data have been permanently deleted.`, type: 'success' });
        setDeleteCustomerId(null);
        setDeleteConfirm(false);
      } else {
        toast({ title: 'Deletion failed', description: 'Customer not found or could not be deleted.', type: 'error' });
      }
    } catch (error) {
      toast({ title: 'Deletion failed', description: error instanceof Error ? error.message : 'An unexpected error occurred.', type: 'error' });
    } finally {
      setDeleteLoading(false);
    }
  };

  const handleDeleteCancel = () => {
    setDeleteCustomerId(null);
    setDeleteConfirm(false);
  };

  return (
  <div className="min-h-screen bg-gray-50 p-4" aria-labelledby="privacy-heading">
      <div className="max-w-3xl mx-auto">
        <div className="flex justify-between items-end mb-8">
          <div>
          <h1 id="privacy-heading" className="text-2xl font-bold text-gray-900">Privacy & Data</h1>
            <p className="text-gray-600">
              Manage your personal data: export a copy of your records or request deletion per privacy law.
            </p>
          </div>
          <Link to="/" className="text-sm text-gray-600 hover:text-gray-900 underline">
            ← Back to Home
          </Link>
        </div>

        {/* Export section */}
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6 mb-8">
          <h2 className="text-lg font-bold mb-4">Data Export</h2>
          <p className="text-sm text-gray-600 mb-4">
            Download a JSON copy of all customer and loyalty data associated with this organization.
          </p>
          <div className="flex gap-3">
            <Button variant="primary" onClick={handleExport} disabled={exportReady}>
              Prepare Export
            </Button>
            {exportReady && (
              <>
                <Button variant="secondary" onClick={downloadExport}>
                  Download JSON
                </Button>
                {customers.length === 0 && (
                  <span className="ml-auto text-xs text-gray-500 flex items-center">
                    No customer data to export
                  </span>
                )}
              </>
            )}
          </div>
          {exportReady && (
            <div className="mt-4 p-3 bg-gray-50 rounded border border-gray-200 text-sm text-gray-600">
              <div className="mb-1"><strong>Customers:</strong> {customers.length}</div>
              <div className="mb-1"><strong>Loyalty accounts:</strong> {accounts.length}</div>
              <div className="mb-1"><strong>Loyalty transactions:</strong> {transactions.length}</div>
              <div><strong>Total outstanding points:</strong> {customers.reduce((s, c) => s + c.loyaltyPoints, 0)}</div>
            </div>
          )}
        </div>

        {/* Deletion section */}
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6 mb-8">
          <h2 className="text-lg font-bold mb-4">Account Deletion</h2>
          <p className="text-sm text-gray-600 mb-4">
            Permanently delete a customer and all associated loyalty data (points, transactions, account). This action cannot be undone.
          </p>

          {!deleteConfirm && !deleteLoading ? (
            <>
              <div className="mb-4">
                <label htmlFor="privacy-delete-customer" className="block text-sm font-medium mb-2">Select customer to delete</label>
                <select
                  id="privacy-delete-customer"
                  value={deleteCustomerId || ''}
                  onChange={(e) => setDeleteCustomerId(e.target.value || null)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                >
                  <option value="">-- Select a customer --</option>
                  {customers.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name} ({c.email || c.phone || 'no contact'})
                    </option>
                  ))}
                </select>
              </div>
              <Button variant="danger" onClick={() => setDeleteConfirm(true)} disabled={!deleteCustomerId || deleteLoading}>
                Delete Customer Data
              </Button>
            </>
          ) : deleteLoading ? (
            <div className="text-center py-4">
              <p className="text-gray-600 text-sm">Deleting data...</p>
            </div>
          ) : (
            <>
              <div className="bg-red-50 border border-red-200 rounded-md p-4 mb-4">
                <h3 className="text-sm font-bold text-red-900 mb-2">Confirm permanent deletion</h3>
                <p className="text-sm text-red-700 mb-3">
                  All loyalty data for this customer will be permanently removed and cannot be recovered.
                </p>
                <div className="flex flex-col sm:flex-row gap-3">
                  <Button variant="secondary" onClick={handleDeleteCancel}>
                    Cancel
                  </Button>
                  <Button variant="danger" onClick={handleDeleteConfirm} disabled={!deleteCustomerId}>
                    Yes, Delete Permanently
                  </Button>
                </div>
              </div>
            </>
          )}
        </div>

        {/* Info section */}
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-6">
          <h2 className="text-lg font-bold text-blue-900 mb-3">Your rights under privacy law</h2>
          <ul className="list-disc list-inside text-sm text-blue-700 space-y-2">
            <li><strong>Right to access:</strong> Export a machine-readable copy of all personal data we hold about a customer.</li>
            <li><strong>Right to erasure:</strong> Request permanent deletion of a customer and all associated loyalty data.</li>
            <li><strong>Data included:</strong> customer profile, loyalty account, points balance, and transaction history.</li>
            <li className="text-xs text-blue-600 mt-2">
              <em>Note: This initial release manages loyalty data in-memory. Order and payment records are managed separately through the Orders system.</em>
            </li>
          </ul>
        </div>

        {/* Consent History section */}
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6 mb-8">
          <h2 className="text-lg font-bold mb-4">Consent History</h2>
          <p className="text-sm text-gray-600 mb-4">
            Audit trail of customer consent given or withdrawn for loyalty program data processing.
          </p>

          <div className="flex flex-col sm:flex-row gap-4 mb-4">
            <div className="flex-1">
              <label htmlFor="privacy-consent-customer" className="block text-sm font-medium text-gray-700 mb-1">Customer</label>
              <select
                id="privacy-consent-customer"
                value={newConsentCustomerId || ''}
                onChange={(e) => setNewConsentCustomerId(e.target.value || null)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
              >
                <option value="">-- Select a customer --</option>
                {customers.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name} ({c.email || c.phone || 'no contact'})
                  </option>
                ))}
              </select>
            </div>
            <div className="flex-1">
              <label htmlFor="privacy-consent-type" className="block text-sm font-medium text-gray-700 mb-1">Consent type</label>
              <select
                id="privacy-consent-type"
                value={newConsentType}
                onChange={(e) => setNewConsentType(e.target.value as ConsentType)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
              >
                <option value="loyalty_enrollment">Loyalty enrollment</option>
                <option value="marketing">Marketing</option>
                <option value="data_processing">Data processing</option>
                <option value="terms_of_service">Terms of service</option>
              </select>
            </div>
            <div className="flex-1">
              <label htmlFor="privacy-consent-given" className="block text-sm font-medium text-gray-700 mb-1">Consent given?</label>
              <select
                id="privacy-consent-given"
                value={newConsentGiven ? 'true' : 'false'}
                onChange={(e) => setNewConsentGiven(e.target.value === 'true')}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
              >
                <option value="true">Yes (given)</option>
                <option value="false">No (withdrawn)</option>
              </select>
            </div>
            <div className="flex-1">
              <label htmlFor="privacy-consent-version" className="block text-sm font-medium text-gray-700 mb-1">Version</label>
              <input
                id="privacy-consent-version"
                value={newConsentVersion}
                onChange={(e) => setNewConsentVersion(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
              />
            </div>
            <Button
              variant="primary"
              onClick={handleRecordConsent}
              disabled={!newConsentCustomerId || recordingConsent}
            >
              {recordingConsent ? 'Recording...' : 'Record Consent'}
            </Button>
          </div>

          <div className="mt-4">
            {consentLoading ? (
              <p className="text-gray-600 text-sm text-center py-2">Loading consent history...</p>
            ) : consentHistory.length === 0 ? (
              <p className="text-gray-500 text-center py-4">No consent records yet.</p>
            ) : (
              <table className="w-full text-left text-sm">
                <thead className="bg-gray-100">
                  <tr>
                    <th className="px-4 py-2 font-semibold text-gray-600">Customer</th>
                    <th className="px-4 py-2 font-semibold text-gray-600">Type</th>
                    <th className="px-4 py-2 font-semibold text-gray-600">Consented</th>
                    <th className="px-4 py-2 font-semibold text-gray-600">Version</th>
                    <th className="px-4 py-2 font-semibold text-gray-600">Date</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {consentHistory.map((c) => {
                    const customer = customersMap.get(c.customer_id);
                    return (
                      <tr key={c.id}>
                        <td className="px-4 py-3 font-medium">{customer?.name || c.customer_id}</td>
                        <td className="px-4 py-3 text-gray-600 capitalize">{c.consent_type}</td>
                        <td className="px-4 py-3">
                          <span
                            className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${
                              c.consented ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                            }`}
                          >
                            {c.consented ? 'Given' : 'Withdrawn'}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-gray-600 text-mono">{c.consent_version}</td>
                        <td className="px-4 py-3 font-mono text-sm">{c.consented_at.slice(0, 16).replace('T', ' ')}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            )}
          </div>
        </div>

        {/* Retention Policy section */}
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6 mb-8">
          <h2 className="text-lg font-bold mb-4">Retention Policy</h2>
          <p className="text-sm text-gray-600 mb-4">
            Define how long loyalty-related data is retained before deletion or review per privacy law.
          </p>

          <div className="flex flex-col sm:flex-row gap-4 mb-4">
            <div className="flex-1">
              <label htmlFor="privacy-retention-data-type" className="block text-sm font-medium text-gray-700 mb-1">Data type</label>
              <select
                id="privacy-retention-data-type"
                value={newPolicyDataType}
                onChange={(e) => setNewPolicyDataType(e.target.value as DataType)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
              >
                <option value="customer_profile">Customer profile</option>
                <option value="loyalty_transactions">Loyalty transactions</option>
                <option value="consent_history">Consent history</option>
                <option value="loyalty_accounts">Loyalty accounts</option>
              </select>
            </div>
            <div className="flex-1">
              <label htmlFor="privacy-retention-days" className="block text-sm font-medium text-gray-700 mb-1">Retention period (days)</label>
              <input
                id="privacy-retention-days"
                type="number"
                min="1"
                value={newPolicyDays}
                onChange={(e) => setNewPolicyDays(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
              />
            </div>
            <div className="flex-1 items-end">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                <input
                  type="checkbox"
                  checked={newPolicyAutoDelete}
                  onChange={(e) => setNewPolicyAutoDelete(e.target.checked)}
                  className="mr-2"
                />
                Auto-delete when expired
              </label>
            </div>
            <Button
              variant="primary"
              onClick={handleSetRetentionPolicy}
              disabled={!newPolicyDays || parseInt(newPolicyDays, 10) <= 0 || settingPolicy}
            >
              {settingPolicy ? 'Setting...' : 'Set Policy'}
            </Button>
          </div>

          <div className="mt-4">
            {retentionLoading ? (
              <p className="text-gray-600 text-sm text-center py-2">Loading retention policies...</p>
            ) : retentionPolicies.length === 0 ? (
              <p className="text-gray-500 text-center py-4">No retention policies set yet.</p>
            ) : (
              <table className="w-full text-left text-sm">
                <thead className="bg-gray-100">
                  <tr>
                    <th className="px-4 py-2 font-semibold text-gray-600">Data type</th>
                    <th className="px-4 py-2 font-semibold text-gray-600 text-right">Retention (days)</th>
                    <th className="px-4 py-2 font-semibold text-gray-600">Auto-delete</th>
                    <th className="px-4 py-2 font-semibold text-gray-600">Updated</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {retentionPolicies.map((p) => (
                    <tr key={p.id}>
                      <td className="px-4 py-3 font-medium capitalize">{p.data_type}</td>
                      <td className="px-4 py-3 text-right font-mono">{p.retention_period_days}</td>
                      <td className="px-4 py-3">
                        <span
                          className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${
                            p.auto_delete ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-700'
                          }`}
                        >
                          {p.auto_delete ? 'Yes' : 'No'}
                        </span>
                      </td>
                      <td className="px-4 py-3 font-mono text-sm">{p.updated_at.slice(0, 10)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          <div className="mt-6 pt-4 border-t border-gray-200">
            <Button
              variant="secondary"
              onClick={handleApplyRetention}
              disabled={applyingRetention}
            >
              {applyingRetention ? 'Applying...' : 'Apply Retention (find expired records)'}
            </Button>
            {expiredRecords.length > 0 && (
              <div className="mt-4">
                <h3 className="text-sm font-semibold text-gray-700 mb-2">Records past retention window:</h3>
                <table className="w-full text-left text-sm">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-4 py-2 font-semibold text-gray-600">Data type</th>
                      <th className="px-4 py-2 font-semibold text-gray-600">Record ID</th>
                      <th className="px-4 py-2 font-semibold text-gray-600 text-right">Days past</th>
                      <th className="px-4 py-2 font-semibold text-gray-600">Action</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {expiredRecords.map((r) => (
                      <tr key={`${r.data_type}-${r.record_id}`}>
                        <td className="px-4 py-3 font-medium capitalize">{r.data_type}</td>
                        <td className="px-4 py-3 font-mono text-sm">{r.record_id}</td>
                        <td className="px-4 py-3 text-right font-mono">{r.days_past_retention}</td>
                        <td className="px-4 py-3 text-gray-600">{r.action}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                <p className="text-xs text-gray-500 mt-2">
                  Records marked "delete" will be removed if auto-delete is enabled. Records marked "review" require manual action.
                </p>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Privacy;
