import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';

export const Profile = () => {
  const { user, loading } = useAuth();
  const [logoutConfirmOpen, setLogoutConfirmOpen] = useState(false);

  if (loading) {
    return <LoadingSpinner message="Loading…" size="lg" />;
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-gray-50 p-4" aria-labelledby="profile-heading">
        <div className="max-w-md mx-auto">
          <div className="bg-white rounded-lg shadow border border-gray-200 p-8 text-center">
            <p id="profile-heading" className="text-gray-900 mb-6">
              Please sign in to view your profile.
            </p>
            <Link
              to="/signin"
              className="inline-block bg-coffee-brown text-white px-4 py-2 rounded hover:bg-gray-800 transition-colors"
            >
              Sign In
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4" aria-labelledby="profile-heading">
      <div className="max-w-md mx-auto">
        <div className="bg-white rounded-lg shadow border border-gray-200 p-8">
          <h1 id="profile-heading" className="text-2xl font-bold text-gray-900 mb-2">
            Profile
          </h1>
          <p className="text-gray-600 mb-6">
            Welcome, {user.name || 'User'}! This is your user profile page.
          </p>
          <Link
            to="/"
            className="text-sm text-gray-600 hover:text-gray-900 underline mb-4 inline-block"
          >
            Home
          </Link>
          <button
            type="button"
            onClick={() => setLogoutConfirmOpen(true)}
            className="bg-gray-100 hover:bg-gray-200 text-gray-900 px-4 py-2 rounded-md text-sm font-medium transition-colors"
          >
            Logout
          </button>
          <ConfirmDialog
            isOpen={logoutConfirmOpen}
            title="Confirm Logout"
            message="Are you sure you want to sign out of your account?"
            confirmLabel="Logout"
            cancelLabel="Cancel"
            variant="danger"
            onConfirm={() => {
              user.logout();
              setLogoutConfirmOpen(false);
            }}
            onCancel={() => setLogoutConfirmOpen(false)}
          />
        </div>
      </div>
    </div>
  );
};

export default Profile;