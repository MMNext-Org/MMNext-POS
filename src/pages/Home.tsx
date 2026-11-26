import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';

export const Home = () => {
  const { loading } = useAuth();

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <LoadingSpinner size="32" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4 sm:p-8">
      <main className="max-w-4xl mx-auto text-center">
        <h1 className="text-3xl font-bold text-gray-900 mb-4">Welcome to BREWFLOW POS</h1>
        <p className="text-lg text-gray-600 mb-6">
          This is the landing page for guests and visitors.
        </p>
        <div className="flex flex-col sm:flex-row gap-3 justify-center">
          <Link
            to="/signin"
            className="bg-coffee-brown text-white px-6 py-3 rounded-md hover:bg-green-600"
          >
            Sign In
          </Link>
          <Link
            to="/signup"
            className="border border-gray-300 text-gray-700 px-6 py-3 rounded-md hover:bg-gray-50"
          >
            Sign Up
          </Link>
        </div>
      </main>
    </div>
  );
};

export default Home;