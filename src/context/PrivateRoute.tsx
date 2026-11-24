import { ReactElement } from 'react';

type AuthState = {
  user: any | null;
  loading: boolean;
};

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
    return <div className="loading">Loading...</div>;
  }
  
  return children;
};

export default PrivateRoute;