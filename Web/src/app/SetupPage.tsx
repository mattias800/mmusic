import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { gql, useMutation } from "urql";

export const createUserMutation = gql`
  mutation CreateUser($username: String!, $password: String!) {
    createUser(input: { username: $username, password: $password }) {
      __typename
      ... on CreateUserSuccess {
        # Assuming a similar success payload
        user {
          id
          username
        }
      }
      ... on CreateUserError {
        # Assuming a similar error payload
        message
      }
    }
  }
`;

export const signInMutation = gql`
  mutation SignIn($username: String!, $password: String!) {
    signIn(input: { username: $username, password: $password }) {
      __typename
      ... on SignInSuccess {
        user {
          id
          username
        }
      }
      ... on SignInError {
        message
      }
    }
  }
`;

export const SetupPage = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const [, createUser] = useMutation(createUserMutation);
  const [, signIn] = useMutation(signInMutation);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsLoading(true);
    setErrorMessage(null);

    try {
      // 1. Create User
      const createUserResult = await createUser({ username, password });

      if (createUserResult.error) {
        throw new Error(
          createUserResult.error.graphQLErrors[0]?.message ||
            createUserResult.error.message ||
            "Error creating user.",
        );
      }

      const createUserData = createUserResult.data?.createUser;
      if (createUserData?.__typename === "CreateUserError") {
        throw new Error(createUserData.message || "Failed to create user.");
      }
      if (createUserData?.__typename !== "CreateUserSuccess") {
        throw new Error("Unexpected error during user creation.");
      }

      console.log("User created:", createUserData.user);

      // 2. Sign In User
      const signInResult = await signIn({ username, password });

      if (signInResult.error) {
        throw new Error(
          signInResult.error.graphQLErrors[0]?.message ||
            signInResult.error.message ||
            "Error signing in.",
        );
      }

      const signInData = signInResult.data?.signIn;
      if (signInData?.__typename === "SignInError") {
        throw new Error(signInData.message || "Failed to sign in.");
      }
      if (signInData?.__typename !== "SignInSuccess") {
        throw new Error("Unexpected error during sign in.");
      }

      console.log("Sign in successful:", signInData.user);

      window.location.reload();
    } catch (error) {
      if (error instanceof Error) {
        setErrorMessage(error.message);
      } else {
        setErrorMessage("Some error");
      }

      console.error("Setup process failed:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100vh",
        backgroundColor: "#f0f2f5",
      }}
    >
      <div
        style={{
          padding: "40px",
          borderRadius: "8px",
          boxShadow: "0 4px 12px rgba(0, 0, 0, 0.1)",
          backgroundColor: "white",
          width: "100%",
          maxWidth: "400px",
        }}
      >
        <h1
          style={{
            textAlign: "center",
            marginBottom: "24px",
            fontSize: "24px",
            fontWeight: "600",
          }}
        >
          Create Admin User
        </h1>
        <form
          onSubmit={handleSubmit}
          style={{ display: "flex", flexDirection: "column", gap: "20px" }}
        >
          <div>
            <Label
              htmlFor="username"
              style={{ marginBottom: "8px", display: "block" }}
            >
              Username
            </Label>
            <Input
              id="username"
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Enter username"
              required
              disabled={isLoading}
            />
          </div>
          <div>
            <Label
              htmlFor="password"
              style={{ marginBottom: "8px", display: "block" }}
            >
              Password
            </Label>
            <Input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter password"
              required
              disabled={isLoading}
            />
          </div>
          {errorMessage && (
            <div
              style={{
                color: "red",
                textAlign: "center",
                marginTop: "10px",
                padding: "10px",
                border: "1px solid red",
                borderRadius: "4px",
                backgroundColor: "#ffeeee",
              }}
            >
              {errorMessage}
            </div>
          )}
          <Button
            type="submit"
            style={{ marginTop: "16px" }}
            disabled={isLoading}
          >
            {isLoading ? "Creating User..." : "Create Admin User"}
          </Button>
        </form>
      </div>
    </div>
  );
};
