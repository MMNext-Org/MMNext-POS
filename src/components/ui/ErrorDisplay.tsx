import { Button } from '@/components/ui/Button';

interface ErrorDisplayProps {
  message: string;
  onRetry?: () => void;
  onDismiss?: () => void;
  retryLabel?: string;
  dismissLabel?: string;
}

export const ErrorDisplay = ({
  message,
  onRetry,
  onDismiss,
  retryLabel = 'Retry',
  dismissLabel = 'Dismiss',
}: ErrorDisplayProps) => (
  <div
    role="alert"
    aria-live="polite"
    className="p-4 mb-4 rounded-md border bg-red-50 border-red-200 text-red-800 text-sm"
  >
    <div className="flex items-start gap-3">
      <span aria-hidden="true">⚠️</span>
      <div className="flex-1">
        <p className="font-medium">{message}</p>
        {onRetry || onDismiss ? (
          <div className="mt-3 flex gap-2">
            {onRetry && (
              <Button variant="secondary" size="sm" onClick={onRetry} aria-label={retryLabel}>
                {retryLabel}
              </Button>
            )}
            {onDismiss && (
              <Button variant="ghost" size="sm" onClick={onDismiss} aria-label={dismissLabel}>
                {dismissLabel}
              </Button>
            )}
          </div>
        ) : null}
      </div>
    </div>
  </div>
);

ErrorDisplay.displayName = 'ErrorDisplay';
