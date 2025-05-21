import React, { useState } from "react";
import { TextInput } from "@/components/inputs/TextInput";
import { PrimaryButton } from "@/components/buttons/PrimaryButton.tsx";
import { ErrorBox } from "@/components/errors/ErrorBox.tsx";

export interface SignInFormValues {
  username: string;
  password: string;
}

interface SignInFormProps {
  onSubmit: (values: SignInFormValues) => void;
  isLoading?: boolean;
  errorMessage?: string;
}

export function SignInForm({
  onSubmit,
  isLoading,
  errorMessage,
}: SignInFormProps) {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

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

      {errorMessage && <ErrorBox message={errorMessage} />}

      <PrimaryButton
        label={"Sign in"}
        loading={isLoading}
        loadingLabel={"Signing in..."}
      />
    </form>
  );
}
