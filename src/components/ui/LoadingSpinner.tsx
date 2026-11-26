import type { SVGProps } from 'react';

interface LoadingSpinnerProps extends SVGProps<SVGSVGElement> {
  size?: 'sm' | 'md' | 'lg';
  message?: string;
}

const sizeClasses: Record<string, string> = {
  sm: 'w-4 h-4',
  md: 'w-6 h-6',
  lg: 'w-8 h-8',
};

export const LoadingSpinner = ({ size = 'md', message, ...props }: LoadingSpinnerProps) => (
  <div className="flex flex-col items-center justify-center gap-2 animate-pulse">
    <svg
      {...props}
      className={`animate-spin rounded-full border-2 border-t-transparent text-gray-600 ${sizeClasses[size]}`}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      aria-label="Loading"
      role="status"
    >
      <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
    </svg>
    {message && <span className="text-sm text-gray-600">{message}</span>}
  </div>
);

LoadingSpinner.displayName = 'LoadingSpinner';
