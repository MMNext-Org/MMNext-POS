import { Button } from './Button';
import { LoadingSpinner } from './LoadingSpinner';

interface ConfirmDialogProps {
  isOpen: boolean;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: 'danger' | 'secondary';
  loading?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export const ConfirmDialog = ({
  isOpen,
  title,
  message,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  variant = 'danger',
  loading = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) => {
  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center"
      role="dialog"
      aria-modal="true"
      aria-labelledby="confirm-dialog-title"
      aria-describedby="confirm-dialog-message"
    >
      <div
        className="fixed inset-0 bg-black/50 backdrop-blur-sm transition-opacity"
        onClick={onCancel}
        aria-hidden="true"
      />
      <div
        className="bg-white rounded-lg shadow-xl border border-gray-200 w-full max-w-md mx-4 p-6 animate-in"
      >
        <h3
          id="confirm-dialog-title"
          className="text-lg font-semibold text-gray-900 mb-2"
        >
          {title}
        </h3>
        <p id="confirm-dialog-message" className="text-gray-600 mb-6">
          {message}
        </p>
        <div className="flex gap-3 justify-end">
          <Button
            variant="ghost"
            onClick={onCancel}
            disabled={loading}
          >
            {cancelLabel}
          </Button>
          <Button
            variant={variant}
            onClick={onConfirm}
            disabled={loading}
          >
            {loading ? <LoadingSpinner size="sm" /> : confirmLabel}
          </Button>
        </div>
      </div>
    </div>
  );
};
