export default function Billing() {
  return (
    <div className="min-h-screen">
      <h1 className="text-2xl font-bold mb-4">Billing & Subscriptions</h1>
      <div className="flex flex-col md:flex-row">
        <div className="w-full md:w-1/3 bg-white p-4 rounded shadow">
          <h2 className="text-xl font-semibold mb-2">Subscription Plans</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="p-4 border rounded">
              <h3 className="text-lg font-bold">Basic Plan</h3>
              <p className="text-gray-600">For small coffee shops</p>
              <ul className="list-disc pl-4">
                <li>Unlimited orders</li>
                <li>1 branch support</li>
                <li>Email support</li>
                <li>Email receipts</li>
              </ul>
              <button className="bg-blue-600 text-white py-2 px-4 rounded-md mt-2">
                Select Plan
              </button>
            </div>
            
            <div className="p-4 border rounded">
              <h3 className="text-lg font-bold">Pro Plan</h3>
              <p className="text-gray-600">For multi-branch operations</p>
              <ul className="list-disc pl-4">
                <li>Unlimited orders</li>
                <li>Multiple branch support</li>
                <li>Priority support</li>
                <li>Detailed analytics</li>
                <li>Custom receipts</li>
                <li>API access</li>
              </ul>
              <button className="bg-blue-600 text-white py-2 px-4 rounded-md mt-2">
                Select Plan
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}