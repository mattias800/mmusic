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
import { Input } from "@/components/ui/input.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";

export interface UserChangePasswordButtonProps {
  user: FragmentType<typeof userChangePasswordUserFragment>;
}

const userChangePasswordUserFragment = graphql(`
  fragment UserChangePasswordButton_User on User {
    id
    username
  }
`);

const setPasswordMutation = graphql(`
  mutation AdminSetUserPassword($input: SetUserPasswordInput!) {
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

export const UserChangePasswordButton: React.FC<
  UserChangePasswordButtonProps
> = (props) => {
  const user = useFragment(userChangePasswordUserFragment, props.user);
  const [open, setOpen] = React.useState(false);
  const [password, setPassword] = React.useState("");
  const [{ fetching }, mutate] = useMutation(setPasswordMutation);

  const onSave = async () => {
    if (!password) {
      return setOpen(false);
    }
    await mutate({
      input: {
        userId: user.id,
        newPassword: password,
      },
    });
    setPassword("");
    setOpen(false);
  };

  return (
    <Dialog open={open} onOpenChange={(v) => (setOpen(v), setPassword(""))}>
      <DialogTrigger asChild>
        <Button variant="blue">Change password</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Change password</DialogTitle>
          <DialogDescription>
            Set a new password for this user.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <label className="block text-sm text-gray-300 mb-1">
              New password
            </label>
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
            />
          </div>
        </div>
        <DialogFooter>
          <Button variant="secondary" onClick={() => setOpen(false)}>
            Cancel
          </Button>
          <Button
            variant="blue"
            loading={fetching}
            onClick={onSave}
            disabled={!password}
          >
            Save
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
