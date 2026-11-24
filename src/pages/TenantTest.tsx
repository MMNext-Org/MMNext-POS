import { useTenant } from '../context/TenantContext';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

export const TenantTest = () => {
  const { selectedOrganization, selectedBranch, loading } = useTenant();
  const { user, loading: authLoading } = useAuth();
  const navigate = useNavigate();

  if (authLoading || loading) {
    return <div>Loading auth and tenant data...</div>;
  }

  return (
    <div className="test-page">
      <h1>Tenant Context Test</h1>
      <p>Auth user: {user ? 'Yes' : 'No'}</p>
      <p>Selected Organization: {selectedOrganization ? selectedOrganization.name : 'None'}</p>
      <p>Selected Branch: {selectedBranch ? selectedBranch.name : 'None'}</p>
      <button onClick={() => navigate('/')}>
        Go to Home
      </button>
    </div>
  );
};

export default TenantTest;