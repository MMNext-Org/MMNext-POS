import { render, screen } from '@testing-library/react';
import { ConnectionTest } from './ConnectionTest';

describe('Connection Status', () => {
  it('should display connection status indicators', () => {
    render(<ConnectionTest />);
    
    const statusElement = screen.getByRole('status');
    expect(statusElement).toBeInTheDocument();
    
    // Check that the component renders the ConnectionStatus component
    const statusComponent = screen.getByTestId('connection-status');
    expect(statusComponent).toBeInTheDocument();
  });
});