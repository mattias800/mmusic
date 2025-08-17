import * as React from "react";
import { useState } from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";
import { Button } from "@/components/ui/button.tsx";
import { ConfirmDeletePrompt } from "@/components/ui/ConfirmDeletePrompt.tsx";

export interface UserDeleteButtonProps {
  user: FragmentType<typeof userDeleteButtonUserFragment>;
}

const userDeleteButtonUserFragment = graphql(`
  fragment UserDeleteButton_User on User {
    id
    username
  }
`);

const deleteUserMutation = graphql(`
  mutation DeleteUser($input: DeleteUserInput!) {
    deleteUser(input: $input) {
      __typename
      ... on DeleteUserSuccess {
        deletedUserId
        user {
          users {
            nodes {
              ...UserAdminPanel_User
            }
          }
        }
      }
      ... on DeleteUserError {
        message
      }
    }
  }
`);

export const UserDeleteButton: React.FC<UserDeleteButtonProps> = (props) => {
  const user = useFragment(userDeleteButtonUserFragment, props.user);

  const [, deleteUser] = useMutation(deleteUserMutation);

  const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);

  return (
    <>
      <div className="mr-auto">
        <Button
          variant="destructive"
          onClick={() => setConfirmDeleteOpen(true)}
        >
          Delete user
        </Button>
      </div>
      {confirmDeleteOpen && (
        <ConfirmDeletePrompt
          itemName={user.username ?? "this user"}
          onClose={() => setConfirmDeleteOpen(false)}
          onConfirmDelete={async () => {
            await deleteUser({ input: { userId: user.id } });
          }}
          promptTitle="Delete user"
          confirmText="Delete"
        />
      )}
    </>
  );
};
