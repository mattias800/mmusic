import { useState } from "react";
import { useMutation } from "urql";
import { SignInForm, SignInFormValues } from "./SignInForm";
import { graphql } from "@/gql";
import { MmusicLogo } from "@/components/logo/MmusicLogo.tsx";
import { Cardy } from "@/components/cards/Cardy.tsx";
import { CenterContent } from "@/components/layout/CenterContent.tsx";

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

export function SignInPanel() {
  const [signInState, signIn] = useMutation(signInMutation);
  const [errorMessage, setErrorMessage] = useState<string | undefined>(
    undefined,
  );

  const onSubmit = async (values: SignInFormValues) => {
    setErrorMessage(undefined);
    const result = await signIn({
      username: values.username,
      password: values.password,
    });

    if (result.error) {
      setErrorMessage(result.error.message || "An unexpected error occurred.");
      console.error("Sign In GQL Error:", result.error);
    } else if (result.data?.signIn?.__typename === "SignInSuccess") {
      window.location.reload();
    } else if (result.data?.signIn?.__typename === "SignInError") {
      setErrorMessage(result.data.signIn.message);
    } else {
      setErrorMessage("An unexpected error occurred during sign in.");
      console.error("Sign In Unexpected Response:", result.data);
    }
  };

  return (
    <CenterContent>
      <div className="sm:mx-auto sm:w-full sm:max-w-md flex flex-col items-center">
        <MmusicLogo width={"250px"} />
        <h2 className="mt-6 text-center text-2xl font-bold leading-9 tracking-tight text-gray-100">
          Sign in to your account
        </h2>
      </div>

      <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-md">
        <Cardy>
          <SignInForm
            onSubmit={onSubmit}
            isLoading={signInState.fetching}
            errorMessage={errorMessage}
          />
        </Cardy>
      </div>
    </CenterContent>
  );
}
