import { useEffect, useState, useRef } from 'react';
import { useAuth } from './context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Routes, Route, Outlet } from 'react-router-dom';
import { Navigation } from './components/Navigation';
import { LoadingSpinner } from './components/ui/LoadingSpinner';
import { Home } from './pages/Home';
import { Dashboard } from './pages/Dashboard';
import { Orders } from './pages/Orders';
import { Profile } from './pages/Profile';
import { SignIn } from './pages/SignIn';
import { SignUp } from './pages/SignUp';
import { TenantTest } from './pages/TenantTest';
import { TenantProvider } from './context/TenantContext';
import { TenantSelector } from './components/TenantSelector';
import { PowerSyncProvider } from './context/PowerSyncContext';  // Changed from ../context/AuthContext
import { getDeviceId } from '@/utils/deviceId';
import { OrderProvider } from '@/context/OrderContext';
import { ProductsProvider } from '@/context/ProductsContext';
import { InventoryProvider } from '@/context/InventoryContext';
import { CssBaseline } from '@chakra-ui/react';
import { PrivateRoute } from './context/PrivateRoute';
import { KitchenDisplay } from './pages/KitchenDisplay';
import { Shifts } from './pages/Shifts';
import { Loyalty } from './pages/Loyalty';
import { Privacy } from './pages/Privacy';
import { Reports } from './pages/Reports';
import { ShiftProvider } from './context/ShiftContext';
import { LoyaltyProvider } from './context/LoyaltyContext';
import { ReportsProvider } from './context/ReportsContext';

function App() {
  const { user, loading: authLoading } = useAuth();
  const navigate = useNavigate();
  const [isAuthenticated, setIsAuthenticated] = useState(false);

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

  useEffect(() => {
    if (!authLoading) {
      setIsAuthenticated(!!user);
    }
  }, [authLoading, user]);

  // Initialize device ID on mount
  useEffect(() => {
    const id = getDeviceId();
    deviceIdRef.current = id;
    console.log('Device ID initialized:', id);
  }, []);

  // Optional: redirect after login logic kept minimal
  useEffect(() => {
    if (!authLoading && user) {
      // In a real app, this would use navigate
    }
  }, [authLoading, user]);

  if (authLoading) {
    return (
      <div className="app flex items-center justify-center min-h-screen">
        <LoadingSpinner size="32" />
      </div>
    );
  }

  return (
    <div className="app">
      <a href="#main-content" className="skip-to-content">
        Skip to main content
      </a>
      <CssBaseline />
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
            <Navigation user={user} />
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
                <Route path="/tenant-test" element={<TenantTest />} />
              </Routes>
            </main>
          </ReportsProvider>
        </LoyaltyProvider>
      </PowerSyncProvider>
    </div>
  );
}

export default App;
