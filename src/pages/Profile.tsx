import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';

export const Profile = () => {
  const { user, loading } = useAuth();

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  if (!user) {
    return (
      <div className="profile">
        <p>Please sign in to view your profile.</p>
        <Link to="/signin">Sign In</Link>
      </div>
    );
};

  return (
    <div className="profile">
      <h1>Profile</h1>
      <p>Welcome, {user.name || 'User'}!</p>
      <p>This is your user profile page.</p>
      <Link to="/">Home</Link>
      <button onClick={() => user?.logout()}>Logout</button>
    </div>
  );
};

export default Profile;