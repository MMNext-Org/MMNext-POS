interface EmptyStateProps {
  title?: string;
  message?: string;
  actionLabel?: string;
  onAction?: () => void;
  icon?: string;
}

const defaultIcons: Record<string, string> = {
  orders: '📦',
  reports: '📊',
  loyalty: '🎖️',
  default: '📭',
};

export const EmptyState = ({
  title = 'No data available',
  message,
  actionLabel,
  onAction,
  icon = defaultIcons.default,
}: EmptyStateProps) => (
  <div className="flex flex-col items-center justify-center gap-3 py-12 text-center">
    <span className="text-4xl">{icon}</span>
    <h3 className="text-lg font-medium text-gray-900">{title}</h3>
    {message && <p className="text-gray-600 text-sm">{message}</p>}
    {onAction && actionLabel && (
      <button
        onClick={onAction}
        className="inline-flex items-center gap-1 px-4 py-2 text-sm font-medium text-gray-900 bg-gray-100 hover:bg-gray-200 rounded-md transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2"
      >
        {actionLabel}
      </button>
    )}
  </div>
);

EmptyState.displayName = 'EmptyState';
