import { useState, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useLoyaltyContext } from '@/context/LoyaltyContext';
import { Button } from '@/components/ui/Button';

type View = 'search' | 'detail';

export const Loyalty = () => {
  const {
    customers,
    enrollCustomer,
    findCustomer,
    searchCustomers,
    getAccount,
    disenrollCustomer,
    earnPoints,
    redeemPoints,
    getCustomerTransactions,
    loading,
    error,
  } = useLoyaltyContext();

  const [view, setView] = useState<View>('search');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCustomer, setSelectedCustomer] = useState<{
    customer: ReturnType<typeof findCustomer>;
    account: ReturnType<typeof getAccount>;
  } | null>(null);
  const [newName, setNewName] = useState('');
  const [newEmail, setNewEmail] = useState('');
  const [newPhone, setNewPhone] = useState('');
  const [earnAmount, setEarnAmount] = useState('');
  const [earnReason, setEarnReason] = useState('');
  const [redeemAmount, setRedeemAmount] = useState('');
  const [redeemReason, setRedeemReason] = useState('');
  const [enrolling, setEnrolling] = useState(false);
  const [earning, setEarning] = useState(false);
  const [redeeming, setRedeeming] = useState(false);
  const [disenrolling, setDisenrolling] = useState(false);
  const earnRef = useRef<HTMLInputElement>(null);
  const redeemRef = useRef<HTMLInputElement>(null);

  const searchResults = searchCustomers(searchQuery);

  const handleEnroll = async () => {
    setEnrolling(true);
    try {
      const customer = await enrollCustomer(newName, newEmail || undefined, newPhone || undefined);
      if (customer) {
        setSelectedCustomer({
          customer,
          account: getAccount(customer.id),
        });
        setView('detail');
        setNewName('');
        setNewEmail('');
        setNewPhone('');
      }
    } finally {
      setEnrolling(false);
    }
  };

  const handleEarn = async () => {
    if (!selectedCustomer) return;
    const { customer } = selectedCustomer;
    const points = parseInt(earnAmount, 10);
    if (!points || points <= 0) return;
    setEarning(true);
    try {
      const ok = await earnPoints(customer.id, points, earnReason || undefined);
      if (ok) {
        setEarnAmount('');
        setEarnReason('');
        setSelectedCustomer({
          customer: findCustomer(customer.id)!,
          account: getAccount(customer.id),
        });
      }
    } finally {
      setEarning(false);
    }
  };

  const handleRedeem = async () => {
    if (!selectedCustomer) return;
    const { customer } = selectedCustomer;
    const points = parseInt(redeemAmount, 10);
    if (!points || points <= 0) return;
    setRedeeming(true);
    try {
      const ok = await redeemPoints(customer.id, points, redeemReason || undefined);
      if (ok) {
        setRedeemAmount('');
        setRedeemReason('');
        setSelectedCustomer({
          customer: findCustomer(customer.id)!,
          account: getAccount(customer.id),
        });
      }
    } finally {
      setRedeeming(false);
    }
  };

  const handleDisenroll = async () => {
    if (!selectedCustomer) return;
    if (!window.confirm('Are you sure you want to disenroll this customer from loyalty? This cannot be undone.')) return;
    setDisenrolling(true);
    try {
      const ok = await disenrollCustomer(selectedCustomer.customer.id);
      if (ok) {
        setSelectedCustomer(null);
        setView('search');
      }
    } finally {
      setDisenrolling(false);
    }
  };

  const customerTransactions = selectedCustomer
    ? getCustomerTransactions(selectedCustomer.customer.id)
    : [];

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-4xl mx-auto">
        <div className="flex justify-between items-end mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Loyalty Program</h1>
            <p className="text-gray-600">
              Enroll customers, earn and redeem points, and manage loyalty accounts.
            </p>
          </div>
          <Link to="/" className="text-sm text-gray-600 hover:text-gray-900 underline">
            ← Back to Home
          </Link>
        </div>

        {view === 'search' ? (
          <>
            <div className="bg-white rounded-lg shadow border border-gray-200 p-6 mb-6">
              <h2 className="text-lg font-bold mb-4">Find or Enroll a Customer</h2>
              <div className="space-y-4">
                <div className="flex gap-3">
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    placeholder="Search by name, email, or phone..."
                    className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                  />
                  <Button variant="secondary" size="sm" onClick={() => setSearchQuery('')}>
                    Clear
                  </Button>
                </div>

                {searchQuery && searchResults.length > 0 && (
                  <div className="border border-gray-200 rounded-md">
                    {searchResults.map((c) => (
                      <button
                        key={c.id}
                        onClick={() => {
                          setSelectedCustomer({
                            customer: c,
                            account: getAccount(c.id),
                          });
                          setView('detail');
                        }}
                        className="w-full text-left px-4 py-3 hover:bg-gray-50 border-b border-gray-100 last:border-b-0"
                      >
                        <div className="font-medium text-gray-900">{c.name}</div>
                        <div className="text-sm text-gray-500">
                          {c.email || c.phone || 'No contact info'}
                        </div>
                      </button>
                    ))}
                  </div>
                )}

                <div className="pt-4 border-t border-gray-200">
                  <h3 className="text-sm font-semibold text-gray-600 mb-3">Or enroll a new customer</h3>
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div className="flex-1">
                      <label className="block text-sm font-medium text-gray-700 mb-1">Full name</label>
                      <input
                        value={newName}
                        onChange={(e) => setNewName(e.target.value)}
                        placeholder="Jane Smith"
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                      />
                    </div>
                    <div className="flex-1">
                      <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                      <input
                        value={newEmail}
                        onChange={(e) => setNewEmail(e.target.value)}
                        placeholder="jane@example.com"
                        type="email"
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                      />
                    </div>
                    <div className="flex-1">
                      <label className="block text-sm font-medium text-gray-700 mb-1">Phone</label>
                      <input
                        value={newPhone}
                        onChange={(e) => setNewPhone(e.target.value)}
                        placeholder="+1 555-0000"
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                      />
                    </div>
                  </div>
                  <Button variant="primary" onClick={handleEnroll} disabled={!newName.trim() || enrolling}>
                    {enrolling ? 'Enrolling...' : 'Enroll Customer'}
                  </Button>
                </div>
              </div>
            </div>

            {customers.length > 0 && (
              <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
                <h2 className="text-lg font-bold mb-4">All Customers</h2>
                <table className="w-full text-left text-sm">
                  <thead className="bg-gray-100">
                    <tr>
                      <th className="px-4 py-2 font-semibold text-gray-600">Name</th>
                      <th className="px-4 py-2 font-semibold text-gray-600">Email</th>
                      <th className="px-4 py-2 font-semibold text-gray-600">Phone</th>
                      <th className="px-4 py-2 font-semibold text-gray-600 text-right">Points</th>
                      <th className="px-4 py-2 font-semibold text-gray-600 text-right">Tier</th>
                      <th className="px-4 py-2 font-semibold text-gray-600">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {customers.map((c) => {
                      const account = getAccount(c.id);
                      return (
                        <tr key={c.id}>
                          <td className="px-4 py-3 font-medium">{c.name}</td>
                          <td className="px-4 py-3 text-gray-600">{c.email || '—'}</td>
                          <td className="px-4 py-3 text-gray-600">{c.phone || '—'}</td>
                          <td className="px-4 py-3 text-right font-mono">{c.loyaltyPoints}</td>
                          <td className="px-4 py-3 text-right">{account?.tier || '—'}</td>
                          <td className="px-4 py-3">
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => {
                                setSelectedCustomer({ customer: c, account });
                                setView('detail');
                              }}
                            >
                              View
                            </Button>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </>
        ) : (
          <>
            <div className="flex justify-between items-end mb-6">
              <div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    setView('search');
                    setSelectedCustomer(null);
                  }}
                >
                  ← Back to Search
                </Button>
                <h1 className="text-2xl font-bold text-gray-900 ml-4 inline-block">
                  {selectedCustomer?.customer.name}
                </h1>
              </div>
              <Button variant="danger" size="sm" onClick={handleDisenroll}>
                Disenroll
              </Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
              <div className="bg-white p-5 rounded-lg shadow border border-gray-200">
                <p className="text-xs font-semibold text-gray-500 uppercase">Member since</p>
                <p className="text-lg font-medium">{selectedCustomer?.customer.createdAt.slice(0, 10)}</p>
              </div>
              <div className="bg-white p-5 rounded-lg shadow border border-gray-200">
                <p className="text-xs font-semibold text-gray-500 uppercase">Points balance</p>
                <p className="text-3xl font-bold text-green-700">
                  {selectedCustomer?.customer.loyaltyPoints}
                </p>
              </div>
              <div className="bg-white p-5 rounded-lg shadow border border-gray-200">
                <p className="text-xs font-semibold text-gray-500 uppercase">Tier</p>
                <p className="text-lg font-medium">{selectedCustomer?.account?.tier || 'Standard'}</p>
              </div>
              <div className="bg-white p-5 rounded-lg shadow border border-gray-200">
                <p className="text-xs font-semibold text-gray-500 uppercase">Total transactions</p>
                <p className="text-lg font-medium">{customerTransactions.length}</p>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
              <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
                <h2 className="text-lg font-bold mb-4">Earn Points</h2>
                <p className="text-sm text-gray-600 mb-4">
                  Award points to this customer for a purchase or activity.
                </p>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium mb-1">Points to earn</label>
                    <input
                      ref={earnRef}
                      value={earnAmount}
                      onChange={(e) => setEarnAmount(e.target.value)}
                      type="number"
                      min="1"
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1">Reason</label>
                    <input
                      value={earnReason}
                      onChange={(e) => setEarnReason(e.target.value)}
                      placeholder="e.g. Coffee purchase"
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                    />
                  </div>
                  <Button variant="primary" onClick={handleEarn} disabled={!earnAmount || parseInt(earnAmount, 10) <= 0 || earning}>
                    {earning ? 'Awarding...' : 'Award Points'}
                  </Button>
                </div>
              </div>

              <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
                <h2 className="text-lg font-bold mb-4">Redeem Points</h2>
                <p className="text-sm text-gray-600 mb-4">
                  Deduct points from this customer's balance.
                </p>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium mb-1">Points to redeem</label>
                    <input
                      ref={redeemRef}
                      value={redeemAmount}
                      onChange={(e) => setRedeemAmount(e.target.value)}
                      type="number"
                      min="1"
                      max={selectedCustomer?.customer.loyaltyPoints || 0}
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                    />
                    <p className="text-xs text-gray-500 mt-1">
                      Available: {selectedCustomer?.customer.loyaltyPoints} points
                    </p>
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1">Reason</label>
                    <input
                      value={redeemReason}
                      onChange={(e) => setRedeemReason(e.target.value)}
                      placeholder="e.g. Free coffee reward"
                      className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500"
                    />
                  </div>
                  <Button
                    variant="secondary"
                    onClick={handleRedeem}
                    disabled={!redeemAmount || parseInt(redeemAmount, 10) <= 0 || parseInt(redeemAmount, 10) > (selectedCustomer?.customer.loyaltyPoints || 0) || redeeming}
                  >
                    {redeeming ? 'Redeeming...' : 'Redeem Points'}
                  </Button>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
              <h2 className="text-lg font-bold mb-4">Transaction History</h2>
              {customerTransactions.length === 0 ? (
                <p className="text-gray-500 text-center py-4">No transactions yet.</p>
              ) : (
                <table className="w-full text-left text-sm">
                  <thead className="bg-gray-100">
                    <tr>
                      <th className="px-4 py-2 font-semibold text-gray-600">Date</th>
                      <th className="px-4 py-2 font-semibold text-gray-600">Type</th>
                      <th className="px-4 py-2 font-semibold text-gray-600 text-right">Points</th>
                      <th className="px-4 py-2 font-semibold text-gray-600 text-right">Balance</th>
                      <th className="px-4 py-2 font-semibold text-gray-600">Reason</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {customerTransactions.map((t) => (
                      <tr key={t.id}>
                        <td className="px-4 py-3 font-mono text-sm">{t.createdAt.slice(0, 16).replace('T', ' ')}</td>
                        <td className="px-4 py-3">
                          <span
                            className={`inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${
                              t.type === 'earn'
                                ? 'bg-green-100 text-green-700'
                                : t.type === 'redeem'
                                  ? 'bg-red-100 text-red-700'
                                  : 'bg-gray-100 text-gray-700'
                            }`}
                          >
                            {t.type === 'earn' ? '+' : t.type === 'redeem' ? '-' : '∼'}{' '}
                            {t.type.charAt(0).toUpperCase() + t.type.slice(1)}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-right font-mono">
                          {t.type === 'earn' ? '+' : ''}{t.points}
                        </td>
                        <td className="px-4 py-3 text-right font-mono">{t.balanceAfter}</td>
                        <td className="px-4 py-3 text-gray-600">{t.reason || '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default Loyalty;
