interface EmptyStateProps {
  title: string;
  description?: string;
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
  title,
  description,
  message,
  actionLabel,
  onAction,
  icon = defaultIcons.default,
}: EmptyStateProps) => (
  <div
    role="status"
    aria-label={title}
    className="flex flex-col items-center justify-center gap-3 py-12 text-center"
  >
    <span className="text-4xl" aria-hidden="true">
      {icon}
    </span>
    <h3 className="text-lg font-medium text-gray-900">{title}</h3>
    {description || message && (
      <p className="mt-2 text-sm text-gray-600">{description || message}</p>
    )}
    {onAction && actionLabel && (
      <button
        onClick={onAction}
        className="mt-6 inline-flex items-center justify-center gap-1 rounded-md border border-transparent bg-gray-900 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-900 disabled:opacity-50 disabled:pointer-events-none"
      >
        {actionLabel}
      </button>
    )}
  </div>
);

EmptyState.displayName = 'EmptyState';
