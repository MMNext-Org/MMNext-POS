import { Button } from './Button';

interface ErrorDisplayProps {
  title?: string;
  message?: string;
  onRetry?: () => void;
  retryLabel?: string;
}

export const ErrorDisplay = ({
  title = 'Error',
  message,
  onRetry,
  retryLabel = 'Retry',
}: ErrorDisplayProps) => (
  <div className="flex flex-col items-center justify-center gap-4 p-6 rounded-lg bg-red-50 border border-red-200 text-center">
    <div className="flex items-center gap-2 text-red-600">
      <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
        <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
      </svg>
      <span className="font-semibold">{title}</span>
    </div>
    {message && <p className="text-gray-700 text-sm">{message}</p>}
    {onRetry && (
      <Button onClick={onRetry} variant="primary" size="sm">
        {retryLabel}
      </Button>
    )}
  </div>
);

ErrorDisplay.displayName = 'ErrorDisplay';
