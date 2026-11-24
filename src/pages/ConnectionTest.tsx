import { ConnectionStatus } from '../components/ConnectionStatus';

export const ConnectionTest = () => {
  return (
    <div className="container" style={{ padding: '2rem', textAlign: 'center' }}>
      <h2>Connection Status Test</h2>
      <ConnectionStatus />
    </div>
  );
};

export default ConnectionTest;