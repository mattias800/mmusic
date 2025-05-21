import React, { useState } from 'react';
import { TextInput, Button, Alert, Stack } from '@mantine/core';
import { AlertCircle } from 'lucide-react';

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
    <form onSubmit={handleSubmit}>
      <Stack gap="lg">
        <TextInput
          label="Username or Email"
          id="username"
          type="text"
          autoComplete="username"
          value={username}
          onChange={(event: React.ChangeEvent<HTMLInputElement>) => setUsername(event.currentTarget.value)}
          required
          placeholder="Enter your username or email"
          disabled={isLoading}
          size="md"
        />

        <TextInput
          label="Password"
          id="password"
          type="password"
          autoComplete="current-password"
          value={password}
          onChange={(event: React.ChangeEvent<HTMLInputElement>) => setPassword(event.currentTarget.value)}
          required
          placeholder="Enter your password"
          disabled={isLoading}
          size="md"
        />

        {errorMessage && (
          <Alert icon={<AlertCircle size="1rem" />} title="Error" color="red" variant="light">
            {errorMessage}
          </Alert>
        )}

        <Button
          type="submit"
          fullWidth
          loading={isLoading}
          disabled={isLoading}
          size="md"
          variant="filled"
        >
          {isLoading ? 'Signing in...' : 'Sign in'}
        </Button>
      </Stack>
    </form>
  );
}
