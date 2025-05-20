import React, { useState } from 'react';
import { TextInput } from '@/components/common/TextInput';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

export interface SignInFormValues {
  username: string;
  password: string;
}

interface SignInFormProps {
  onSubmit: (values: SignInFormValues) => void;
  isLoading?: boolean;
  errorMessage?: string;
}

export function SignInForm({ onSubmit, isLoading, errorMessage }: SignInFormProps) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    onSubmit({ username, password });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <TextInput
          label="Username or Email"
          id="username"
          type="text"
          autoComplete="username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
          placeholder="Enter your username or email"
          disabled={isLoading}
        />
      </div>

      <div>
        <TextInput
          label="Password"
          id="password"
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          placeholder="Enter your password"
          disabled={isLoading}
        />
      </div>

      {errorMessage && (
        <div className="p-3 rounded-md bg-red-900/30 border border-red-700">
          <p className="text-sm text-red-400">{errorMessage}</p>
        </div>
      )}

      <div>
        <Button
          type="submit"
          className={cn(
            'w-full flex justify-center rounded-md px-3 py-2.5 text-sm font-semibold leading-6 shadow-sm',
            isLoading
              ? 'bg-gray-500 hover:bg-gray-500 cursor-not-allowed'
              : 'bg-indigo-600 hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 text-white'
          )}
          disabled={isLoading}
        >
          {isLoading ? 'Signing in...' : 'Sign in'}
        </Button>
      </div>
    </form>
  );
} 