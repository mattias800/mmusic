import * as React from "react";
import { SecondaryButton } from "@/components/buttons/SecondaryButton.tsx";
import { graphql } from "@/gql";
import { useMutation } from "urql";
import { useNavigate } from "react-router";

export interface SignOutButtonProps {}

export const signOutMutation = graphql(`
  mutation SignOut {
    signOut {
      __typename

      ... on SignOutSuccess {
        success
      }
      ... on SignOutError {
        message
      }
    }
  }
`);

export const SignOutButton: React.FC<SignOutButtonProps> = () => {
  const [{ fetching }, signOut] = useMutation(signOutMutation);
  const navigate = useNavigate();

  const onClick = async () => {
    await signOut({});
    navigate("/");
    window.location.reload();
  };

  return (
    <SecondaryButton label={"Sign out"} loading={fetching} onClick={onClick} />
  );
};
