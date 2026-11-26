import { SVGProps } from 'react';

interface LoadingSpinnerProps extends SVGProps<SVGSVGElement> {
  size?: number;
  color?: string;
}

export const LoadingSpinner = ({ size = 24, color = 'currentColor', ...props }: LoadingSpinnerProps) => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    width={size}
    height={size}
    viewBox="0 0 24 24"
    fill="none"
    stroke={color}
    strokeWidth="2"
    strokeLinecap="round"
    strokeLinejoin="round"
    className="animate-spin"
    {...props}
  >
    <circle cx="12" cy="12" r="10" />
    <path d="M12 6a6 6 0 1 0 0 12 6 6 0 0 0 0-12Z" />
  </svg>
);

LoadingSpinner.displayName = 'LoadingSpinner';
