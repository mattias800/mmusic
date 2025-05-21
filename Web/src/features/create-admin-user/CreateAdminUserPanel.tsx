import * as React from "react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useMutation } from "urql";
import { CREATE_USER_MUTATION, SIGN_IN_MUTATION } from "@/gql/misc.ts";
import { showNotification } from "@mantine/notifications";
import { Alert, Button, Stack, TextInput } from "@mantine/core";
import { AlertCircle } from "lucide-react";

export interface CreateAdminUserPanelProps {}

export const CreateAdminUserPanel: React.FC<CreateAdminUserPanelProps> = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const navigate = useNavigate();

  const [, createUserMutation] = useMutation(CREATE_USER_MUTATION);
  const [, signInMutation] = useMutation(SIGN_IN_MUTATION);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsLoading(true);
    setErrorMessage(null);

    try {
      const createUserResult = await createUserMutation({ username, password });
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
      showNotification({
        title: "User Created",
        message: `User ${createUserData.user.username} created successfully. Signing in...`,
        color: "green",
      });

      const signInResult = await signInMutation({ username, password });
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
      showNotification({
        title: "Sign In Successful",
        message: `Welcome ${signInData.user.username}! Redirecting...`,
        color: "green",
      });

      navigate("/", { replace: true });
      window.location.reload();
    } catch (error) {
      if (error instanceof Error) {
        setErrorMessage(error.message);
      } else {
        setErrorMessage("Error when creating admin user.");
      }
      console.error("Setup process failed:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <Stack gap="lg">
        <TextInput
          label="Username"
          id="username"
          type="text"
          value={username}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
            setUsername(e.currentTarget.value)
          }
          placeholder="Enter username"
          required
          disabled={isLoading}
          size="md"
        />
        <TextInput
          label="Password"
          id="password"
          type="password"
          value={password}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
            setPassword(e.currentTarget.value)
          }
          placeholder="Enter password"
          required
          disabled={isLoading}
          size="md"
        />
        {errorMessage && (
          <Alert
            icon={<AlertCircle size="1rem" />}
            title="Error"
            color="red"
            variant="outline"
          >
            {errorMessage}
          </Alert>
        )}
        <Button
          type="submit"
          fullWidth
          loading={isLoading}
          disabled={isLoading}
          size="md"
          mt="md" // Mantine prop for margin-top
        >
          {isLoading ? "Creating User..." : "Create Admin User"}
        </Button>
      </Stack>
    </form>
  );
};
