import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';

export const SignIn = () => {
  const { login, loading } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await login(email, password);
      navigate('/dashboard');
    } catch (err) {
      setError((err as Error).message);
    }
  };

  return (
    <section className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
      <div className="max-w-md w-full bg-white rounded-lg shadow border border-gray-200 p-8">
        <h2 className="text-2xl font-bold text-gray-900 mb-6 text-center">Sign In</h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
              Email
            </label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500 focus:outline-none"
              aria-describedby="email-hint"
            />
            <p id="email-hint" className="mt-1 text-xs text-gray-500">
              We'll never share your email with anyone else.
            </p>
          </div>
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
              Password
            </label>
            <div className="relative">
              <input
                id="password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500 focus:outline-none pr-10"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                aria-label={showPassword ? 'Hide password' : 'Show password'}
                aria-pressed={showPassword}
              >
                {showPassword ? (
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.06M18.42 5.16c-.11-.11-.21-.22-.32-.33-.12-.12-.24-.23-.36-.34-.26-.28-.55-.56-.89-.84-.56-.47-1.18-.91-1.84-1.31-.3-.18-.59-.37-.88-.54a2 2 0 0 0-.66-.24 2 2 0 0 0-.6-.08H7.12c-.41 0-.79.09-1.15.27a3 3 0 0 0-.66.61c-.15.3-.24.66-.27 1.02-.04.51-.04 1.02-.04 1.55 0 .53 0 1.04.04 1.55.03.36.12.72.27 1.02.08.27.17.53.27.78.09.22.22.42.36.6.16.21.34.4.55.56.17.13.35.25.54.35.3.16.62.3.96.42.56.2 1.14.37 1.76.5.24.05.47.09.71.12.36.04.72.06 1.08.06.35 0 .69-.02 1.03-.06.42-.07.82-.17 1.22-.3.21-.07.43-.14.63-.22.4-.17.81-.34 1.2-.54.26-.13.5-.29.74-.45.24-.17.49-.33.72-.52.23-.18.45-.38.66-.59.18-.18.35-.37.5-.57a9 9 0 0 0 .64-1.01c.08-.18.14-.37.2-.55.1-.34.17-.7.2-1.06.01-.15.02-.3.02-.45s-.01-.3-.02-.45z" />
                  </svg>
                ) : (
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                    <path d="M12 18a6 6 0 0 0 6-6 6 6 0 0 0-6-6 6 6 0 0 0-6 6 6 6 0 0 0 6 6z" />
                  </svg>
                )}
              </button>
            </div>
          </div>
          {error && (
            <p className="text-sm text-red-600" role="alert" aria-live="polite">
              {error}
            </p>
          )}
          <button
            type="submit"
            disabled={loading}
            className="w-full bg-coffee-brown text-white px-4 py-2 rounded hover:bg-gray-800 disabled:opacity-50 disabled:pointer-events-none focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-coffee-brown"
          >
            {loading ? 'Logging in...' : 'Login'}
          </button>
          {loading && <LoadingSpinner size="sm" message="Signing in…" />}
          <p className="mt-4 text-center text-sm text-gray-600">
            Don't have an account?{' '}
            <Link to="/signup" className="text-coffee-brown hover:text-coffee-brown/80 underline medium">
              Sign up
            </Link>
          </p>
        </form>
      </div>
    </section>
  );
};