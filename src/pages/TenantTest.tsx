import { useTenant } from '../context/TenantContext';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';

export const TenantTest = () => {
  const { selectedOrganization, selectedBranch, loading } = useTenant();
  const { user, loading: authLoading } = useAuth();
  const navigate = useNavigate();

  if (authLoading || loading) {
    return <LoadingSpinner message="Loading auth and tenant data…" />;
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-md mx-auto">
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
          <h1 className="text-xl font-bold text-gray-900 mb-4">
            Tenant Context Test
          </h1>
          <p className="text-sm text-gray-600 mb-2">
            Auth user: {user ? 'Yes' : 'No'}
          </p>
          <p className="text-sm text-gray-600 mb-2">
            Selected Organization:{' '}
            {selectedOrganization ? selectedOrganization.name : 'None'}
          </p>
          <p className="text-sm text-gray-600 mb-4">
            Selected Branch: {selectedBranch ? selectedBranch.name : 'None'}
          </p>
          <button
            onClick={() => navigate('/')}
            className="bg-coffee-brown text-white px-4 py-2 rounded hover:bg-gray-800 transition-colors text-sm"
          >
            Go to Home
          </button>
        </div>
      </div>
    </div>
  );
};

export default TenantTest;