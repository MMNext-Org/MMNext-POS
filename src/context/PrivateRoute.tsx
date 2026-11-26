import type { ReactElement } from 'react';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';

interface PrivateRouteProps {
  children: ReactElement;
  isAuthenticated: boolean;
}

const PrivateRoute = ({ children, isAuthenticated }: PrivateRouteProps) => {
  if (isAuthenticated === false) {
    return <div className="404">
      Access denied! Please sign in to continue.
    </div>;
  }
  
  if (isAuthenticated === null) {
    // Loading state - handle this as needed
    return <LoadingSpinner message="Loading…" />;
  }
  
  return children;
};

export default PrivateRoute;