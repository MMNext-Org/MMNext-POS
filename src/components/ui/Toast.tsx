import { createContext, useContext, useCallback, useState, type ReactNode } from 'react';

type ToastType = 'success' | 'error' | 'warning' | 'info';

interface Toast {
  id: string;
  title: string;
  description?: string;
  type: ToastType;
  duration?: number;
}

interface ToastContextType {
  toast: (options: Omit<Toast, 'id'>) => void;
  dismiss: (id: string) => void;
  toasts: Toast[];
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

export const useToast = () => {
  const context = useContext(ToastContext);
  if (context === undefined) {
    throw new Error('useToast must be used within a ToastProvider');
  }
  return context;
};

interface ToastProviderProps {
  children: ReactNode;
}

export const ToastProvider = ({ children }: ToastProviderProps) => {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const toast = useCallback((options: Omit<Toast, 'id'>) => {
    const id = Math.random().toString(36).substr(2, 9);
    const newToast: Toast = { id, ...options };
    setToasts((prev) => [...prev, newToast]);

    // Auto-dismiss after duration (default 5000ms)
    const duration = options.duration ?? 5000;
    setTimeout(() => {
      dismiss(id);
    }, duration);

    // Store timeout ref for cleanup if manually dismissed
    // (not needed for simple implementation; timeout handles cleanup)
    return id;
  }, []);

  const dismiss = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  return (
    <ToastContext.Provider value={{ toast, dismiss, toasts }}>
      {children}
      <ToastContainer toasts={toasts} onDismiss={dismiss} />
    </ToastContext.Provider>
  );
};

type ToastVariantClasses = Record<ToastType, string>;

const variantClasses: ToastVariantClasses = {
  success: 'bg-green-500/10 border-green-500/20 text-green-800',
  error: 'bg-red-500/10 border-red-500/20 text-red-800',
  warning: 'bg-amber-500/10 border-amber-500/20 text-amber-800',
  info: 'bg-blue-500/10 border-blue-500/20 text-blue-800',
};

const iconByType: Record<ToastType, string> = {
  success: '✓',
  error: '✕',
  warning: '!',
  info: 'i',
};

interface ToastContainerProps {
  toasts: Toast[];
  onDismiss: (id: string) => void;
}

const ToastContainer = ({ toasts, onDismiss }: ToastContainerProps) => {
  return (
    <div className="fixed top-4 right-4 z-50 flex flex-col gap-2 pointer-events-none">
      {toasts.map((t) => (
        <div
          key={t.id}
          className={`pointer-events-auto flex items-start gap-3 px-4 py-3 rounded-lg border shadow-lg animate-in fade-in slide-in-from-right-2 duration-300 ${
            variantClasses[t.type]
          }`}
          role="alert"
          aria-live="polite"
        >
          <span className="flex-shrink-0 w-5 h-5 rounded-full bg-white/20 flex items-center justify-center text-sm font-bold mt-0.5">
            {iconByType[t.type]}
          </span>
          <div className="flex-1 min-w-0">
            <h4 className="font-semibold text-sm">{t.title}</h4>
            {t.description && (
              <p className="text-xs opacity-90 mt-0.5">{t.description}</p>
            )}
          </div>
          <button
            onClick={() => onDismiss(t.id)}
            className="flex-shrink-0 p-1 hover:bg-white/20 rounded transition-colors"
            aria-label="Dismiss notification"
          >
            ✕
          </button>
        </div>
      ))}
    </div>
  );
};
