import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import { SyncQueue } from '@/components/SyncQueue';

export const Dashboard = () => {
  const { user, loading } = useAuth();

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  if (!user) {
    // Redirect to Sign In if not authenticated
    // This could be handled via navigation, but for simplicity we'll just show a message
    return (
      <div className="dashboard">
        <p>Please sign in to access the dashboard.</p>
        <Link to="/signin">Sign In</Link>
      </div>
    );
  }

  return (
    <div className="dashboard">
      <h1>Dashboard</h1>
      <p>Welcome, {user.name || 'User'}!</p>
      <p>This is the main dashboard for authenticated users.</p>
      <Link to="/orders">Orders</Link>
      <Link to="/profile">Profile</Link>
      <SyncQueue />
      <button onClick={() => user?.logout()}>Logout</button>
    </div>
  );
};

export default Dashboard;