import { useState } from 'react';
import CustomerList from '../components/CustomerList';
import { useMediaQuery } from '@/utils/Responsive';

export default function Layout() {
  const [currentPage, setCurrentPage] = useState<string>('dashboard');
  const isMobile = useMediaQuery(768); // 768px breakpoint for tablet and phone (common breakpoint for mobile devices)

  const handleNavigate = (route: string) => {
    setCurrentPage(route);
    console.log(`Navigate to ${route}`);
  };

  return (
    <div className="min-h-screen">
      <header className="bg-gray-800 text-white p-4 md:p-6">
        <h1 className="text-xl font-bold md:text-2xl">BREWFLOW SaaS Console</h1>
      </header>
      <main className="flex flex-col md:flex-row flex-1 mobile:px-4">
        {/* Mobile: navigation at bottom with better touch targets */}
        className={`md:w-1/4 bg-gray-700 text-white p-4 md:p-3 order-last md:order-first ${isMobile ? 'w-full' : 'w-1/4'} text-center mobile:flex-col mobile:items-center mobile:space-y-2 mobile:py-5 mobile:bg-gray-600 mobile:text-white mobile:rounded-t-lg mobile:min-w-16 mobile:shadow-md`}
          aria-label="Main navigation">
          <ul className={`space-y-1 md:space-y-2 ${isMobile ? 'py-2' : 'py-1'} ${isMobile ? 'flex-row' : 'column'}`}>
            <li className={`py-${isMobile ? 2 : 1} md:py-2 mobile:flex mobile:justify-center`} onClick={() => handleNavigate('dashboard')}>
                          <a 
                            href="#" 
                            className="hover:underline md:inline-block ${isMobile ? 'block' : 'inline-block'} px-5 py-3 block mobile:px-5 mobile:py-5 mobile:rounded-lg mobile:min-w-10"
                            aria-current="page"
                            aria-label="Dashboard (current page)">
                            {isMobile ? 'Dashboard' : 'Dashboard'}
                          </a>
            </li>
            <li className={`py-${isMobile ? 2 : 1} md:py-2 mobile:flex mobile:justify-center`} onClick={() => handleNavigate('tenants')}>
                          <a 
                            href="#" 
                                                      className="hover:underline md:inline-block ${isMobile ? 'block' : 'inline-block'} px-4 py-2 block mobile:px-5 mobile:py-5 mobile:rounded-lg mobile:min-w-10"
                            aria-label="Tenant Management">
                            {isMobile ? 'Tenant Management' : 'Tenant Management'}
            </li>
            <li className={`py-${isMobile ? 2 : 1} md:py-2 mobile:flex mobile:justify-center`} onClick={() => handleNavigate('billing')}>
                          <a 
                            href="#" 
                                                      className="hover:underline md:inline-block ${isMobile ? 'block' : 'inline-block'} px-4 py-2 block mobile:px-5 mobile:py-5 mobile:rounded-lg mobile:min-w-10"
                            aria-label="Billing & Subscriptions">
                            {isMobile ? 'Billing & Subscriptions' : 'Billing & Subscriptions'}
            </li>
            <li className={`py-${isMobile ? 2 : 1} md:py-2 mobile:flex mobile:justify-center`} onClick={() => handleNavigate('customers')}>
                          <a 
                            href="#" 
                                                      className="hover:underline md:inline-block ${isMobile ? 'block' : 'inline-block'} px-4 py-2 block mobile:px-5 mobile:py-5 mobile:rounded-lg mobile:min-w-10"
                            aria-label="Customers">
                            {isMobile ? 'Customers' : 'Customers'}
                          </a>
            </li>
            <li className={`py-${isMobile ? 2 : 1} md:py-2 mobile:flex mobile:justify-center`} onClick={() => handleNavigate('reports')}>
                          <a 
                            href="#" 
                                                      className="hover:underline md:inline-block ${isMobile ? 'block' : 'inline-block'} px-4 py-2 block mobile:px-5 mobile:py-5 mobile:rounded-lg mobile:min-w-10"
                            aria-label="Reports">
                            {isMobile ? 'Reports' : 'Reports'}
            </li>
            <li className={`py-${isMobile ? 2 : 1} md:py-2 mobile:flex mobile:justify-center`} onClick={() => handleNavigate('settings')}>
                          <a 
                            href="#" 
                                                      className="hover:underline md:inline-block ${isMobile ? 'block' : 'inline-block'} px-4 py-2 block mobile:px-5 mobile:py-5 mobile:rounded-lg mobile:min-w-10"
                            aria-label="Settings">
                            {isMobile ? 'Settings' : 'Settings'}
                          </a>
            </li>
          </ul>
        </nav>
              <div className="flex-1 p-3 mobile:p-5 md:p-6 lg:p-8">
          {currentPage === 'dashboard' && <Dashboard />}
          {currentPage === 'tenants' && <TenantManagement />}
          {currentPage === 'billing' && <Billing />}
          {currentPage === 'customers' && <CustomerList />}
          {currentPage === 'reports' && <Reports />}
          {/* Add more pages as needed */}
        </div>
      </main>
          </div>
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
