/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ['class', '[data-theme="dark"]'],
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#2563eb',
          foreground: '#ffffff',
        },
        success: '#16a34a',
        warn: '#f59e0b',
        danger: '#ef4444',
      },
      borderRadius: {
        xl: '1rem',
        '2xl': '1.25rem',
      },
    },
  },
  plugins: [require('@tailwindcss/forms')],
};


