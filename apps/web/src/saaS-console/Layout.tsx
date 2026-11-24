import { useState } from 'react';
import CustomerList from '../components/CustomerList';

export default function Layout() {
  const [currentPage, setCurrentPage] = useState<string>('dashboard');

  const handleNavigate = (route: string) => {
    setCurrentPage(route);
    console.log(`Navigate to ${route}`);
  };

  return (
    <div className="min-h-screen">
      <header className="bg-gray-800 text-white p-4">
        <h1 className="text-xl font-bold">BREWFLOW SaaS Console</h1>
      </header>
      <main className="flex flex-col md:flex-row flex-1">
        <nav className="md:w-1/4 bg-gray-700 text-white p-4">
          <ul className="space-y-4">
            <li className="py-2" onClick={() => handleNavigate('dashboard')}>
              <a href="#" className="hover:underline">Dashboard</a>
            </li>
            <li className="py-2" onClick={() => handleNavigate('tenants')}>
              <a href="#" className="hover:underline">Tenant Management</a>
            </li>
            <li className="py-2" onClick={() => handleNavigate('billing')}>
              <a href="#" className="hover:underline">Billing & Subscriptions</a>
            </li>
            <li className="py-2" onClick={() => handleNavigate('customers')}>
              <a href="#" className="hover:underline">Customers</a>
            </li>
            <li className="py-2" onClick={() => handleNavigate('reports')}>
              <a href="#" className="hover:underline">Reports</a>
            </li>
            <li className="py-2" onClick={() => handleNavigate('settings')}>
              <a href="#" className="hover:underline">Settings</a>
            </li>
          </ul>
        </nav>
        <div className="flex-1 p-4">
          {currentPage === 'dashboard' && <Dashboard />}
          {currentPage === 'tenants' && <TenantManagement />}
          {currentPage === 'billing' && <Billing />}
          {currentPage === 'customers' && <CustomerList />}
          {currentPage === 'reports' && <Reports />}
          {/* Add more pages as needed */}
        </div>
      </main>
    </div>
  );
}

// Placeholder components - in a real app these would be actual components
function Dashboard() {
  return <div className="text-center py-8">Dashboard Page</div>;
}

function TenantManagement() {
  return <div className="text-center py-8">Tenant Management Page</div>;
}

function Billing() {
  return <div className="text-center py-8">Billing & Subscriptions Page</div>;
}

function Reports() {
  return <div className="text-center py-8">Reports Page</div>;
}