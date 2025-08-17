import * as React from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog.tsx";
import { Button } from "@/components/ui/button.tsx";
import { Pencil } from "lucide-react";
import { Input } from "@/components/ui/input.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";

export interface UserEditButtonProps {
  user: FragmentType<typeof userEditButtonUserFragment>;
}

const userEditButtonUserFragment = graphql(`
  fragment UserEditButton_User on User {
    id
    username
  }
`);

const updateUsernameMutation = graphql(`
  mutation UpdateUserUsername($input: UpdateUserUsernameInput!) {
    updateUserUsername(input: $input) {
      __typename
      ... on UpdateUserUsernameSuccess {
        user {
          ...UserAdminPanel_User
        }
      }
      ... on UpdateUserUsernameError {
        message
      }
    }
  }
`);

const setPasswordMutation = graphql(`
  mutation SetUserPassword($input: SetUserPasswordInput!) {
    setUserPassword(input: $input) {
      __typename
      ... on SetUserPasswordSuccess {
        user {
          ...UserAdminPanel_User
        }
      }
      ... on SetUserPasswordError {
        message
      }
    }
  }
`);

export const UserEditButton: React.FC<UserEditButtonProps> = (props) => {
  const user = useFragment(userEditButtonUserFragment, props.user);

  const [, updateUsername] = useMutation(updateUsernameMutation);
  const [, setPassword] = useMutation(setPasswordMutation);

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button size="sm" variant="blue" iconLeft={Pencil}>
          Edit
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit User</DialogTitle>
          <DialogDescription>
            Update username or set a new password for this user.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <label className="block text-sm text-gray-300 mb-1">Username</label>
            <Input
              defaultValue={user.username ?? ""}
              onBlur={async (e) => {
                const name = e.target.value.trim();
                if (name && name !== user.username) {
                  await updateUsername({
                    input: {
                      userId: user.id,
                      newUsername: name,
                    },
                  });
                }
              }}
            />
          </div>
          <div>
            <label className="block text-sm text-gray-300 mb-1">
              New password
            </label>
            <Input
              type="password"
              placeholder="Set new password"
              onKeyDown={async (e) => {
                if (e.key === "Enter") {
                  const value = (e.target as HTMLInputElement).value;
                  if (value) {
                    await setPassword({
                      input: {
                        userId: user.id,
                        newPassword: value,
                      },
                    });
                    (e.target as HTMLInputElement).value = "";
                  }
                }
              }}
            />
          </div>
        </div>
        <DialogFooter>
          <Button
            variant="blue"
            onClick={() => (document.activeElement as HTMLElement)?.blur()}
          >
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
