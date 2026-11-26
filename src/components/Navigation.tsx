import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import { ConnectionStatus } from './ConnectionStatus';

const MANAGER_ROLES = ['Shift Lead', 'Branch Manager'];

export const Navigation = ({
  user,
  colorMode = 'light',
  toggleColorMode,
}: {
  user: any | null;
  colorMode?: 'light' | 'dark';
  toggleColorMode?: () => void;
}) => {
  const userRole = user?.app_metadata?.role_name || user?.role || null;
  const canViewReports = userRole ? MANAGER_ROLES.includes(userRole) : false;

  return (
    <nav className="navbar" aria-label="Main navigation">
      <div className="nav-brand">
        <Link to="/" aria-label="BREWFLOW POS home">
          BREWFLOW POS
        </Link>
      </div>
      <div className="nav-links">
        <Link to="/" aria-current="page">
          Home
        </Link>
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
            <button onClick={() => user?.logout()}>Logout</button>
            <button
              onClick={toggleColorMode}
              aria-label={colorMode === 'light' ? 'Switch to dark mode' : 'Switch to light mode'}
            >
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