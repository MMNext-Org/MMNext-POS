import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import { ConnectionStatus } from './ConnectionStatus';

const MANAGER_ROLES = ['Shift Lead', 'Branch Manager'];

export const Navigation = ({ user }: { user: any | null }) => {
  const userRole = user?.app_metadata?.role_name || user?.role || null;
  const canViewReports = userRole ? MANAGER_ROLES.includes(userRole) : false;

  return (
    <nav className="navbar">
      <div className="nav-brand">
        <Link to="/">BREWFLOW POS</Link>
      </div>
      <div className="nav-links">
        <Link to="/">Home</Link>
        <ConnectionStatus />
        {user ? (
          <>
            <Link to="/dashboard">Dashboard</Link>
            <Link to="/orders">Orders</Link>
            <Link to="/shifts">Shifts</Link>
            <Link to="/loyalty">Loyalty</Link>
            <Link to="/privacy">Privacy</Link>
            {canViewReports && <Link to="/reports">Reports</Link>}
            <Link to="/profile">Profile</Link>
            <Link to="/tenant-test">Tenant Test</Link>
            <button onClick={() => user?.logout()}>Logout</button>
            <button onClick={toggleColorMode}>
              {colorMode === 'light' ? '🌙' : '☀️'}
            </button>
          </>
        ) : (
          <>
            <Link to="/signin">Sign In</Link>
            <Link to="/signup">Sign Up</Link>
          </>
        )}
      </div>
    </nav>
  );
};