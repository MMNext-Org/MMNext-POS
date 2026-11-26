interface EmptyStateProps {
  title: string;
  description?: string;
  actionLabel?: string;
  onAction?: () => void;
  icon?: string;
}

export const EmptyState = ({
  title,
  description,
  actionLabel,
  onAction,
  icon = '📭',
}: EmptyStateProps) => (
  <div
    role="status"
    aria-label={title}
    className="text-center py-12"
  >
    <div className="text-4xl mb-4" aria-hidden="true">
      {icon}
    </div>
    <h3 className="text-lg font-medium text-gray-900">{title}</h3>
    {description && (
      <p className="mt-2 text-sm text-gray-600">{description}</p>
    )}
    {onAction && actionLabel && (
      <button
        onClick={onAction}
        className="mt-6 inline-flex items-center justify-center rounded-md border border-transparent bg-gray-900 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-900 disabled:opacity-50 disabled:pointer-events-none"
      >
        {actionLabel}
      </button>
    )}
  </div>
);

EmptyState.displayName = 'EmptyState';
