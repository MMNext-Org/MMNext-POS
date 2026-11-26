import { useEffect, useState, useCallback } from 'react';
import { useAuth } from './context/AuthContext';
import { Routes, Route } from 'react-router-dom';
import { Navigation } from './components/Navigation';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';
import { Home } from './pages/Home';
import { Dashboard } from './pages/Dashboard';
import Orders from './pages/Orders';
import { Profile } from './pages/Profile';
import { SignIn } from './pages/SignIn';
import { SignUp } from './pages/SignUp';
import { Shifts } from './pages/Shifts';
import { Loyalty } from './pages/Loyalty';
import { Privacy } from './pages/Privacy';
import { Reports } from './pages/Reports';
import { TenantProvider } from './context/TenantContext';
import { TenantSelector } from './components/TenantSelector';
import { PowerSyncProvider } from './context/PowerSyncContext';
import { OrderProvider } from '@/context/OrderContext';
import { ProductsProvider } from '@/context/ProductsContext';
import { ShiftProvider } from './context/ShiftContext';
import { LoyaltyProvider } from './context/LoyaltyContext';
import { ReportsProvider } from './context/ReportsContext';
import { ToastProvider } from './components/ui/Toast';

function App() {
  const { user, loading: authLoading } = useAuth();
  const [colorMode, setColorMode] = useState<'light' | 'dark'>('light');

  const toggleColorMode = useCallback(() => {
    setColorMode((prev) => (prev === 'light' ? 'dark' : 'light'));
  }, []);

  // Initialize PowerSync client once on app mount
  useEffect(() => {
    const initializePowerSyncClient = async () => {
      try {
        // Import the initialization function
        const { initializePowerSync } = await import('@/utils/powersyncClient');
        
        // Initialize PowerSync
        await initializePowerSync();
        console.log('PowerSync initialized successfully');
      } catch (error) {
        console.error('Error initializing PowerSync:', error);
        // Handle initialization error appropriately (e.g., show error message)
      }
    };
    
    // Only initialize after auth loading is complete
    if (!authLoading) {
      initializePowerSyncClient();
    }
  }, [authLoading]);

  if (authLoading) {
    return (
      <div className="app flex items-center justify-center min-h-screen">
        <LoadingSpinner size="32" />
      </div>
    );
  }

  return (
    <div className="app">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:top-4 focus:left-4 focus:z-50 focus:px-4 focus:py-2 focus:bg-gray-900 focus:text-white"
      >
        Skip to main content
      </a>
      <ToastProvider>
        <PowerSyncProvider>
        <OrderProvider>
          <ShiftProvider>
            <ProductsProvider>
              <TenantProvider>
                <TenantSelector />
              </TenantProvider>
            </ProductsProvider>
          </ShiftProvider>
        </OrderProvider>
        <LoyaltyProvider>
          <ReportsProvider>
            <Navigation user={user} colorMode={colorMode} toggleColorMode={toggleColorMode} />
            <main id="main-content" tabIndex={-1}>
              <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/dashboard" element={<Dashboard />} />
                <Route path="/orders" element={<Orders />} />
                <Route path="/profile" element={<Profile />} />
                <Route path="/shifts" element={<Shifts />} />
                <Route path="/loyalty" element={<Loyalty />} />
                <Route path="/privacy" element={<Privacy />} />
                <Route path="/reports" element={<Reports />} />
                <Route path="/signin" element={<SignIn />} />
                <Route path="/signup" element={<SignUp />} />
              </Routes>
            </main>
          </ReportsProvider>
        </LoyaltyProvider>
      </PowerSyncProvider>
      </ToastProvider>
    </div>
  );
}

export default App;
