import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';

export const Home = () => {
  const { loading } = useAuth();

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  return (
    <div className="home">
      <h1>Welcome to BREWFLOW POS</h1>
      <p>This is the landing page for guests and visitors.</p>
      <p>
        <Link to="/signin">Sign In</Link> |{' '}
        <Link to="/signup">Sign Up</Link>
      </p>
    </div>
  );
};

export default Home;