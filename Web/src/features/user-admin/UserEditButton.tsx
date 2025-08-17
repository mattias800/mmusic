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
// import { Input } from "@/components/ui/input.tsx";
// import { graphql } from "@/gql";

export interface UserEditButtonProps {
  user: unknown;
}

// kept for reference of previous behavior; not used anymore
// const userEditButtonUserFragment = graphql(`
//   fragment UserEditButton_User on User {
//     id
//     username
//   }
// `);

// kept for reference of previous behavior; not used anymore
// const updateUsernameMutation = graphql(`
//   mutation UpdateUserUsername($input: UpdateUserUsernameInput!) {
//     updateUserUsername(input: $input) {
//       __typename
//       ... on UpdateUserUsernameSuccess {
//         user {
//           ...UserAdminPanel_User
//         }
//       }
//       ... on UpdateUserUsernameError {
//         message
//       }
//     }
//   }
// `);

// kept for reference of previous behavior; not used anymore
// const setPasswordMutation = graphql(`
//   mutation SetUserPassword($input: SetUserPasswordInput!) {
//     setUserPassword(input: $input) {
//       __typename
//       ... on SetUserPasswordSuccess {
//         user {
//           ...UserAdminPanel_User
//         }
//       }
//       ... on SetUserPasswordError {
//         message
//       }
//     }
//   }
// `);

export const UserEditButton: React.FC<UserEditButtonProps> = () => {
  // const user = useFragment(userEditButtonUserFragment, props.user);
  // const [, updateUsername] = useMutation(updateUsernameMutation);
  // const [, setPassword] = useMutation(setPasswordMutation);

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
          <div className="text-sm text-gray-400">This dialog is deprecated. Use Change username/password buttons.</div>
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
