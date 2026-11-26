import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import { SyncQueue } from '@/components/SyncQueue';

export const Dashboard = () => {
  const { user, loading } = useAuth();

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-gray-50 p-4 sm:p-8">
        <main className="max-w-4xl mx-auto">
          <h1 className="text-2xl font-bold text-gray-900 mb-4">Dashboard</h1>
          <p className="text-gray-600">Please sign in to access the dashboard.</p>
          <Link to="/signin" className="text-coffee-brown hover:underline">
            Sign In
          </Link>
        </main>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4 sm:p-8">
      <main className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold text-gray-900 mb-4">Dashboard</h1>
        <p className="text-gray-600 mb-6">
          Welcome, {user.name || 'User'}! This is the main dashboard for authenticated users.
        </p>
        <div className="flex flex-col sm:flex-row gap-3 mb-6">
          <Link
            to="/orders"
            className="bg-coffee-brown text-white px-4 py-2 rounded-md hover:bg-green-600"
          >
            Orders
          </Link>
          <Link
            to="/profile"
            className="border border-gray-300 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-50"
          >
            Profile
          </Link>
        </div>
        <SyncQueue aria-label="Sync queue status" />
        <button
          onClick={() => user?.logout()}
          className="bg-red-100 text-red-700 px-4 py-2 rounded-md hover:bg-red-200"
          aria-label="Logout"
        >
          Logout
        </button>
      </main>
    </div>
  );
};

export default Dashboard;