/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        text: '#6b6375',
        'text-h': '#08060d',
        bg: '#fff',
        border: '#e5e4e7',
        'code-bg': '#f4f3ec',
        accent: '#aa3bff',
        'accent-bg': 'rgba(170, 59, 255, 0.1)',
        'accent-border': 'rgba(170, 59, 255, 0.5)',
        'social-bg': 'rgba(244, 243, 236, 0.5)',
        'coffee-brown': '#795548',
      },
      fontFamily: {
        sans: ['system-ui', 'Segoe UI', 'Roboto', 'sans-serif'],
        heading: ['system-ui', 'Segoe UI', 'Roboto', 'sans-serif'],
        mono: ['ui-monospace', 'Consolas', 'monospace'],
      },
      boxShadow: {
        DEFAULT: 'rgba(0, 0, 0, 0.1) 0 10px 15px -3px, rgba(0, 0, 0, 0.05) 0 4px 6px -2px',
      },
      screens: {
        sm: '640px',
        md: '768px',
        lg: '1024px',
        xl: '1280px',
      },
    },
  },
  plugins: [],
}
