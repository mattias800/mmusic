import React from 'react';
import { cn } from '@/lib/utils'; // Assuming this path is correct as per guidelines

export interface TextInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}

const TextInput = React.forwardRef<HTMLInputElement, TextInputProps>(
  ({ className, type = 'text', label, id, error, ...props }, ref) => {
    const inputId = id || (label ? label.toLowerCase().replace(/\s+/g, '-') : undefined);

    return (
      <div className="w-full">
        {label && (
          <label htmlFor={inputId} className="block text-sm font-medium text-gray-300 mb-1">
            {label}
          </label>
        )}
        <input
          type={type}
          id={inputId}
          className={cn(
            'block w-full rounded-md border-0 py-2.5 px-3 bg-gray-700 text-white shadow-sm placeholder-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6',
            error ? 'ring-2 ring-inset ring-red-500 focus:ring-red-500' : 'ring-1 ring-inset ring-gray-600 focus:ring-indigo-500',
            className
          )}
          ref={ref}
          {...props}
        />
        {error && <p className="mt-1 text-xs text-red-400">{error}</p>}
      </div>
    );
  }
);

TextInput.displayName = 'TextInput';

export { TextInput }; 