import { ConnectionStatus } from '../components/ConnectionStatus';

export const ConnectionTest = () => {
  return (
    <div className="container p-8 text-center">
      <h2 className="text-xl font-bold mb-4 text-coffee-brown">Connection Status Test</h2>
      <ConnectionStatus />
    </div>
  );
};

export default ConnectionTest;