import React, { useState } from "react";
import { TextInput } from "@/components/inputs/TextInput";
import { MessageBox } from "@/components/errors/MessageBox.tsx";
import { Button } from "@/components/ui/button.tsx";
import { LogIn } from "lucide-react";

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
    <form onSubmit={handleSubmit} className="flex flex-col gap-6">
      <div>
        <TextInput
          label="Username"
          id="username"
          type="text"
          autoComplete="username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
          placeholder="Enter your username"
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

      {errorMessage && <MessageBox message={errorMessage} variant={"error"} />}

      <Button loading={isLoading} iconLeft={LogIn}>
        {isLoading ? "Signing in..." : "Sign in"}
      </Button>
    </form>
  );
}
