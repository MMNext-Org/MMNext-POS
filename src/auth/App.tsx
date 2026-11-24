import { useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Navigation } from '../components/Navigation';
import { Home } from '../pages/Home';
import { Dashboard } from '../pages/Dashboard';
import { SignIn } from '../pages/SignIn';

function App() {
  const { user, loading } = useAuth();

  // If loading, show loading state
  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  // Render content based on authentication status
  return (
    <div className="app">
      <Navigation />
      <main>
        {user ? (
          <Dashboard />
        ) : (
          <>
            <SignIn />
            <Home />
          </>
        )}
      </main>
    </div>
  );
}

export default App;