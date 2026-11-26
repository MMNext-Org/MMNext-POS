import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';
import { Button } from '@/components/ui/Button';

export const Home = () => {
  const { loading } = useAuth();

  if (loading) {
    return <LoadingSpinner message="Loading…" size="lg" />;
  }

  return (
    <div className="home min-h-screen bg-gray-50" aria-labelledby="home-heading">
      <header className="bg-white border-b border-gray-200">
        <div className="max-w-5xl mx-auto px-4 py-6">
          <h1 id="home-heading" className="text-3xl font-bold text-gray-900">
            Welcome to BREWFLOW POS
          </h1>
          <p className="text-gray-600 mt-2">
            The point-of-sale system for modern coffee shops and cafés.
          </p>
        </div>
      </header>
      <main className="max-w-5xl mx-auto px-4 py-12">
        <section className="grid md:grid-cols-2 gap-8 items-center">
          <div>
            <h2 className="text-2xl font-semibold text-gray-900 mb-4">
              Streamline your café operations
            </h2>
            <p className="text-gray-600 mb-6">
              Manage orders, shifts, inventory, loyalty, and reports from one
              place. Built for coffee shops that care about speed and accuracy.
            </p>
            <div className="flex flex-col sm:flex-row gap-4">
              <Link to="/signin">
                <Button variant="primary">Sign In</Button>
              </Link>
              <Link to="/signup">
                <Button variant="secondary">Create an Account</Button>
              </Link>
            </div>
          </div>
          <div className="bg-white rounded-lg shadow border border-gray-200 p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              Quick actions
            </h3>
            <ul className="space-y-3 text-sm text-gray-700">
              <li>
                <Link
                  to="/orders"
                  className="text-coffee-brown hover:underline font-medium"
                >
                  View &amp; Create Orders
                </Link>
              </li>
              <li>
                <Link
                  to="/dashboard"
                  className="text-coffee-brown hover:underline font-medium"
                >
                  Open Dashboard
                </Link>
              </li>
              <li>
                <Link
                  to="/loyalty"
                  className="text-coffee-brown hover:underline font-medium"
                >
                  Manage Loyalty
                </Link>
              </li>
              <li>
                <Link
                  to="/reports"
                  className="text-coffee-brown hover:underline font-medium"
                >
                  View Reports
                </Link>
              </li>
            </ul>
          </div>
        </section>
      </main>
    </div>
  );
};

export default Home;