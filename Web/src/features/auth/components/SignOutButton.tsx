import * as React from "react";
import { graphql } from "@/gql";
import { useMutation } from "urql";
import { useNavigate } from "react-router";
import { Button } from "@/components/ui/button.tsx";
import { LogOut } from "lucide-react";

export interface SignOutButtonProps {}

const signOutMutation = graphql(`
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
    <Button
      variant={"default"}
      size={"default"}
      loading={fetching}
      onClick={onClick}
      iconLeft={LogOut}
    >
      Sign out
    </Button>
  );
};
