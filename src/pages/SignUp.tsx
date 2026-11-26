import { useState, useMemo } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';
import { LoadingSpinner } from '@/components/ui/LoadingSpinner';

type StrengthLevel = { label: string; color: string; bg: string; width: string };

function computePasswordStrength(pw: string): StrengthLevel {
  let score = 0;
  if (pw.length >= 8) score += 1;
  if (pw.length >= 12) score += 1;
  if (/[A-Z]/.test(pw)) score += 1;
  if (/[0-9]/.test(pw)) score += 1;
  if (/[^A-Za-z0-9]/.test(pw)) score += 1;

  if (pw.length === 0) return { label: '', color: 'text-gray-400', bg: 'bg-gray-200', width: 'w-0' };
  if (score <= 2) return { label: 'Weak', color: 'text-red-600', bg: 'bg-red-200', width: 'w-1/4' };
  if (score <= 3) return { label: 'Fair', color: 'text-yellow-600', bg: 'bg-yellow-200', width: 'w-2/4' };
  if (score <= 4) return { label: 'Good', color: 'text-blue-600', bg: 'bg-blue-200', width: 'w-3/4' };
  return { label: 'Strong', color: 'text-green-600', bg: 'bg-green-200', width: 'w-full' };
}

export const SignUp = () => {
  const { register, loading } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');
  const [error, setError] = useState('');

  const passwordStrength = useMemo(() => computePasswordStrength(password), [password]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      await register(email, password, name);
      navigate('/dashboard');
    } catch (err) {
      setError((err as Error).message);
    }
  };

  return (
    <section className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
      <div className="max-w-md w-full bg-white rounded-lg shadow border border-gray-200 p-8">
        <h2 className="text-2xl font-bold text-gray-900 mb-2 text-center">Create Account</h2>
        <p className="text-sm text-gray-600 mb-6 text-center">
          Fill in your details to create an account
        </p>
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
            <input
              id="password"
              type="password"
              autoComplete="new-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500 focus:outline-none"
            />
            {password.length > 0 && (
              <div className="mt-2 space-y-1">
                <div className="flex justify-between text-xs">
                  <span className={`font-medium ${passwordStrength.color}`}>
                    {passwordStrength.label}
                  </span>
                </div>
                <div className="h-1.5 rounded-full overflow-hidden bg-gray-200">
                  <div
                    className={`h-full transition-all duration-300 ${passwordStrength.bg} ${passwordStrength.width}`}
                    role="progressbar"
                    aria-valuenow={passwordStrength.width === 'w-full' ? 100 : passwordStrength.width === 'w-3/4' ? 75 : passwordStrength.width === 'w-2/4' ? 50 : 25}
                    aria-valuemin={0}
                    aria-valuemax={100}
                    aria-label="Password strength"
                  />
                </div>
                {passwordStrength.label === 'Weak' && (
                  <p className="text-xs text-red-600">
                    Use 8+ characters with uppercase, numbers, and symbols.
                  </p>
                )}
              </div>
            )}
          </div>
          <div>
            <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
              Full Name <span className="text-gray-400 font-normal text-xs">(optional)</span>
            </label>
            <input
              id="name"
              type="text"
              autoComplete="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Jane Smith"
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-gray-500 focus:outline-none"
            />
            <p className="mt-1 text-xs text-gray-500">
              Optional — helps us personalize your experience.
            </p>
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
            {loading ? 'Creating account...' : 'Create Account'}
          </button>
          {loading && <LoadingSpinner size="sm" message="Creating account…" />}
          <p className="mt-4 text-center text-sm text-gray-600">
            Already have an account?{' '}
            <Link to="/signin" className="text-coffee-brown hover:text-coffee-brown/80 underline">
              Sign in
            </Link>
          </p>
        </form>
      </div>
    </section>
  );
};

export default SignUp;