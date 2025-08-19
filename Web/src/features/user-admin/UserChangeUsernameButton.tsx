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

export interface UserChangeUsernameButtonProps {
  user: FragmentType<typeof userChangeUsernameUserFragment>;
}

const userChangeUsernameUserFragment = graphql(`
  fragment UserChangeUsernameButton_User on User {
    id
    username
  }
`);

const updateUsernameMutation = graphql(`
  mutation AdminUpdateUserUsername($input: UpdateUserUsernameInput!) {
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

export const UserChangeUsernameButton: React.FC<
  UserChangeUsernameButtonProps
> = (props) => {
  const user = useFragment(userChangeUsernameUserFragment, props.user);
  const [open, setOpen] = React.useState(false);
  const [username, setUsername] = React.useState(user.username ?? "");
  const [{ fetching }, mutate] = useMutation(updateUsernameMutation);

  const onSave = async () => {
    const name = username.trim();
    if (!name || name === user.username) return setOpen(false);
    await mutate({
      input: {
        userId: user.id,
        newUsername: name,
      },
    });
    setOpen(false);
  };

  return (
    <Dialog
      open={open}
      onOpenChange={(v) => (setOpen(v), setUsername(user.username ?? ""))}
    >
      <DialogTrigger asChild>
        <Button variant="blue">Change username</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Change username</DialogTitle>
          <DialogDescription>Provide a new unique username.</DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <label className="block text-sm text-gray-300 mb-1">Username</label>
            <Input
              value={username}
              onChange={(e) => setUsername(e.target.value)}
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
            disabled={!username.trim()}
          >
            Save
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
