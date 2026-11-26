import { Navigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';

const PrivateRoute = () => {
  const { user, loading } = useAuth();

  if (loading) {
    return <LoadingSpinner message="Loading…" />;
  }

  return user ? <Navigate to="/dashboard" /> : <Navigate to="/signin" />;
};

export default PrivateRoute;