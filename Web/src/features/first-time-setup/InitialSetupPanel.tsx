import * as React from "react";
import { useState } from "react";
import { useMutation } from "urql";
import { CenterContent } from "@/components/layout/CenterContent.tsx";
import { MmusicLogo } from "@/components/logo/MmusicLogo.tsx";
import { Cardy } from "@/components/cards/Cardy.tsx";
import { Heading } from "@/components/text/Heading.tsx";
import { Txt } from "@/components/text/Txt.tsx";
import { Label } from "@/components/ui/label.tsx";
import { Input } from "@/components/ui/input.tsx";
import { graphql } from "@/gql";
import { Button } from "@/components/ui/button.tsx";

export interface InitialSetupPanelProps {}

const createUserMutation = graphql(`
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
`);

const signInMutation = graphql(`
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
`);

export const InitialSetupPanel: React.FC<InitialSetupPanelProps> = () => {
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
    <CenterContent>
      <div className="sm:mx-auto sm:w-full sm:max-w-md flex flex-col items-center">
        <MmusicLogo width={"250px"} />
      </div>

      <Cardy className={"gap-4"}>
        <div className={"flex justify-center"}>
          <Heading>Welcome to Mmusic!</Heading>
        </div>

        <Txt>To get started, please create an admin account.</Txt>

        <div />

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
          <div />
          <Button type="submit" loading={isLoading} variant={"blue"}>
            {isLoading ? "Creating User..." : "Create admin user"}
          </Button>
        </form>
      </Cardy>
    </CenterContent>
  );
};
