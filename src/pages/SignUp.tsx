import { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Form, Input, Button, Box, Alert } from '@chakra-ui/react';

export const SignUp = () => {
  const { register, registrationLoading, loading } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');
  const [error, setError] = useState('');

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
    <Box maxW="300px" mx="auto" mt="100px" p="4">
      <Box textAlign="center">
        <Box mb="6">
          <Box fontSize="2xl" fontWeight="bold">
            Create Account
          </Box>
        </Box>
        <Box mb="6">
          <Box fontSize="sm" fontWeight="medium">
            Fill in your details to create an account
          </Box>
        </Box>
        <form onSubmit={handleSubmit}>
          <Box mb="4">
            <Input
              type="email"
              placeholder="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </Box>
          <Box mb="4">
            <Input
              type="password"
              placeholder="Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </Box>
          <Box mb="4">
            <Input
              placeholder="Full Name (optional)"
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
          </Box>
          <Box mb="6">
            <Button
              type="submit"
              colorScheme="teal"
              isLoading={loading || registrationLoading}
              onClick={handleSubmit}
            >
              {loading || registrationLoading ? 'Creating account...' : 'Create Account'}
            </Button>
          </Box>
          {error && <Alert status="error" mt="4">{error}</Alert>}
        </form>
      </Box>
    </Box>
  );
};

export default SignUp;