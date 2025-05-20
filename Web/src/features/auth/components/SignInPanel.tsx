import { useState } from "react";
import { useMutation } from "urql";
import { SignInForm, SignInFormValues } from "./SignInForm";
import { graphql } from "@/gql";
import { MmusicLogo } from "@/components/logo/MmusicLogo.tsx";

// import { useRouter } from 'next/router';

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
      alert(`Sign In successful! Welcome ${result.data.signIn.user.username}`);
      // TODO: Handle successful sign in
    } else if (result.data?.signIn?.__typename === "SignInError") {
      setErrorMessage(result.data.signIn.message);
    } else {
      setErrorMessage("An unexpected error occurred during sign in.");
      console.error("Sign In Unexpected Response:", result.data);
    }
  };

  return (
    <div className="flex min-h-full flex-col justify-center px-6 py-12 lg:px-8 bg-gray-900 text-white">
      <div className="sm:mx-auto sm:w-full sm:max-w-md flex flex-col items-center">
        <MmusicLogo width={"250px"} />
        <h2 className="mt-6 text-center text-2xl font-bold leading-9 tracking-tight text-gray-100">
          Sign in to your account
        </h2>
      </div>

      <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-md">
        <div className="bg-gray-800 px-6 py-8 shadow sm:rounded-lg sm:px-10">
          <SignInForm
            onSubmit={handleSignInSubmit}
            isLoading={signInState.fetching}
            errorMessage={errorMessage}
          />
        </div>
      </div>
    </div>
  );
}
