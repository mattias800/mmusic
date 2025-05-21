import { useState } from "react";
import { useMutation } from "urql";
import { SignInForm, SignInFormValues } from "./SignInForm";
import { graphql } from "@/gql";
import { MmusicLogo } from "@/common/components/logo/MmusicLogo.tsx";
import { Paper, Title, Center, Stack, Container } from "@mantine/core";
import { showNotification } from "@mantine/notifications";

export const signInMutation = graphql(`
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

export function SignInPanel() {
  const [signInState, signIn] = useMutation(signInMutation);
  const [errorMessage, setErrorMessage] = useState<string | undefined>(
    undefined,
  );
  // const router = useRouter();

  const handleSignInSubmit = async (values: SignInFormValues) => {
    setErrorMessage(undefined);
    const result = await signIn({
      username: values.username,
      password: values.password,
    });

    if (result.error) {
      setErrorMessage(result.error.message || "An unexpected error occurred.");
      console.error("Sign In GQL Error:", result.error);
    } else if (result.data?.signIn?.__typename === "SignInSuccess") {
      console.log("Sign In successful:", result.data.signIn.user);
      showNotification({
        title: "Sign In Successful",
        message: `Welcome ${result.data.signIn.user.username}!`,
        color: "green",
      });
      // TODO: Handle successful sign in
    } else if (result.data?.signIn?.__typename === "SignInError") {
      setErrorMessage(result.data.signIn.message);
    } else {
      setErrorMessage("An unexpected error occurred during sign in.");
      console.error("Sign In Unexpected Response:", result.data);
    }
  };

  return (
    <Container fluid className="flex min-h-full flex-col justify-center py-12 lg:px-8 text-white">
      <Paper withBorder shadow="md" p="xl" radius="md" className="sm:mx-auto sm:w-full sm:max-w-md bg-gray-900">
        <Stack align="center">
          <MmusicLogo width={"200px"} />
          <Title order={2} className="mt-6 text-center font-bold leading-9 tracking-tight text-gray-100">
            Sign in to your account
          </Title>
        </Stack>

        <div className="mt-10">
          <SignInForm
            onSubmit={handleSignInSubmit}
            isLoading={signInState.fetching}
            errorMessage={errorMessage}
          />
        </div>
      </Paper>
    </Container>
  );
}
