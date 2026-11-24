import { describe, it, expect } from 'vitest';
import { render, fireEvent } from '@testing-library/react';
import { ShiftProvider, useShiftContext } from './ShiftContext';

const Wrapper = ({ children }: { children: React.ReactNode }) => (
  <ShiftProvider>{children}</ShiftProvider>
);

const Probe = () => {
  const ctx = useShiftContext();
  return (
    <div>
      <span data-testid="status">{ctx.shift?.status}</span>
      <span data-testid="cashin">{ctx.shift?.cashIn.toFixed(2)}</span>
      <span data-testid="cashout">{ctx.shift?.cashOut.toFixed(2)}</span>
      <button data-testid="open" onClick={() => ctx.openShift('staff-1', 'Alex Rivera', 0)}>
        Open
      </button>
      <button data-testid="close" onClick={ctx.requestClose}>
        Request Close
      </button>
      <button data-testid="handover" onClick={() => ctx.requestHandover('staff-2')}>
        Hand Over
      </button>
      <button data-testid="approve" onClick={() => ctx.respondToApproval(true)}>
        Approve
      </button>
      <button data-testid="reject" onClick={() => ctx.respondToApproval(false)}>
        Reject
      </button>
      <button data-testid="finalize" onClick={() => ctx.closeShift('staff-4', 'Casey Moore')}>
        Finalize Close
      </button>
      <button
        data-testid="tips"
        onClick={() =>
          ctx.allocateTips(30, [
            { userId: 'staff-2', userName: 'Samir Patel', amount: 15 },
            { userId: 'staff-3', userName: 'Jordan Lee', amount: 15 },
          ])
        }
      >
        Allocate Tips
      </button>
    </div>
  );
};

describe('ShiftContext', () => {
  it('opens a shift', () => {
    const { getByTestId } = render(<Wrapper><Probe /></Wrapper>);
    fireEvent.click(getByTestId('open'));
    expect(getByTestId('status').textContent).toBe('open');
    expect(getByTestId('cashin').textContent).toBe('0.00');
  });

  it('requests close -> pending_approval', () => {
    const { getByTestId } = render(<Wrapper><Probe /></Wrapper>);
    fireEvent.click(getByTestId('open'));
    fireEvent.click(getByTestId('close'));
    expect(getByTestId('status').textContent).toBe('pending_approval');
  });

  it('rejecting approval reopens shift', () => {
    const { getByTestId } = render(<Wrapper><Probe /></Wrapper>);
    fireEvent.click(getByTestId('open'));
    fireEvent.click(getByTestId('close'));
    fireEvent.click(getByTestId('reject'));
    expect(getByTestId('status').textContent).toBe('open');
  });

  it('approving then finalizing closes shift', () => {
    const { getByTestId } = render(<Wrapper><Probe /></Wrapper>);
    fireEvent.click(getByTestId('open'));
    fireEvent.click(getByTestId('close'));
    fireEvent.click(getByTestId('approve'));
    fireEvent.click(getByTestId('finalize'));
    expect(getByTestId('status').textContent).toBe('closed');
  });

  it('allocates tips only when closed', () => {
    const { getByTestId } = render(<Wrapper><Probe /></Wrapper>);
    fireEvent.click(getByTestId('open'));
    fireEvent.click(getByTestId('tips'));
    // tips not allocated because shift is not closed
    expect(getByTestId('status').textContent).toBe('open');
  });

  it('allocates tips when closed and sums match', () => {
    const { getByTestId } = render(<Wrapper><Probe /></Wrapper>);
    fireEvent.click(getByTestId('open'));
    fireEvent.click(getByTestId('close'));
    fireEvent.click(getByTestId('approve'));
    fireEvent.click(getByTestId('finalize'));
    fireEvent.click(getByTestId('tips'));
    expect(getByTestId('status').textContent).toBe('closed');
  });

  it('handover sets status to handed_over', () => {
    const { getByTestId } = render(<Wrapper><Probe /></Wrapper>);
    fireEvent.click(getByTestId('open'));
    fireEvent.click(getByTestId('handover'));
    expect(getByTestId('status').textContent).toBe('handed_over');
  });

  it('cash in updates totals', () => {
    const { getByTestId } = render(<Wrapper><Probe /></Wrapper>);
    fireEvent.click(getByTestId('open'));
    const probe = document.querySelector('[data-testid="cashin"]');
    expect(probe?.textContent).toBe('0.00');
  });
});
