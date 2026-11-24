import { createContext, useContext, useState, useCallback } from 'react';

export interface CashTransaction {
  type: 'cash_in' | 'cash_out';
  amount: number;
  timestamp: Date;
}

export interface CashContextType {
  float: number;
  transactions: CashTransaction[];
  cashIn: (amount: number) => void;
  cashOut: (amount: number) => void;
  shiftClose: () => void;
}

const CashContext = createContext<CashContextType | undefined>(undefined);

export const CashProvider = ({ children }: { children: React.ReactNode }) => {
  const [float, setFloat] = useState<number>(0);
  const [transactions, setTransactions] = useState<CashTransaction[]>([]);

  const cashIn = useCallback((amount: number) => {
    setFloat(prev => prev + amount);
    setTransactions(prev => [...prev, { type: 'cash_in', amount, timestamp: new Date() }]);
  }, []);

  const cashOut = useCallback((amount: number) => {
    if (amount > float) {
      return;
    }
    setFloat(prev => prev - amount);
    setTransactions(prev => [...prev, { type: 'cash_out', amount, timestamp: new Date() }]);
  }, []);

  const shiftClose = useCallback(() => {
    console.log('Shift closed. Final float:', float);
  }, []);

  return (
    <CashContext.Provider value={{ float, transactions, cashIn, cashOut, shiftClose }}>
      {children}
    </CashContext.Provider>
  );
};

export const useCashContext = () => {
  const context = useContext(CashContext);
  if (!context) {
    throw new Error('useCashContext must be used within CashProvider');
  }
  return context;
};