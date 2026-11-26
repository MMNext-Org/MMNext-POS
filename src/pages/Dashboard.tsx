import { useAuth } from '../context/AuthContext';
import { Link, useNavigate } from 'react-router-dom';
import { SyncQueue } from '@/components/SyncQueue';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';

export const Dashboard = () => {
  const { user, loading } = useAuth();
  const navigate = useNavigate();

  if (loading) {
    return <LoadingSpinner message="Loading…" size="lg" />;
  }

  if (!user) {
    navigate('/signin');
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4" aria-labelledby="dashboard-heading">
      <div className="max-w-4xl mx-auto">
        <div className="flex justify-between items-end mb-8">
          <div>
            <h1 className="text-2xl font-bold text-gray-900" id="dashboard-heading">
              Dashboard
            </h1>
            <p className="text-gray-600">
              Welcome, {user.name || 'User'}! This is the main dashboard for
              authenticated users.
            </p>
          </div>
          <Link
            to="/"
            className="text-sm text-gray-600 hover:text-gray-900 underline"
          >
            ← Back to Home
          </Link>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <Link
            to="/orders"
            className="block bg-white rounded-lg shadow border border-gray-200 p-6 hover:shadow-md transition-shadow"
          >
            <h2 className="text-lg font-semibold text-gray-900 mb-2">Orders</h2>
            <p className="text-sm text-gray-500">View and create orders</p>
          </Link>
          <Link
            to="/profile"
            className="block bg-white rounded-lg shadow border border-gray-200 p-6 hover:shadow-md transition-shadow"
          >
            <h2 className="text-lg font-semibold text-gray-900 mb-2">Profile</h2>
            <p className="text-sm text-gray-500">Manage your account</p>
          </Link>
        </div>

        <SyncQueue />

        <div className="mt-8">
          <button
            onClick={() => user?.logout()}
            className="bg-gray-100 hover:bg-gray-200 text-gray-900 px-4 py-2 rounded-md text-sm font-medium transition-colors"
          >
            Logout
          </button>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;