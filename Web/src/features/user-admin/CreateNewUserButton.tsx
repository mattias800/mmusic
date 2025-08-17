import * as React from "react";
import { useState } from "react";
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
import { useMutation } from "urql";
import { graphql } from "@/gql";

export interface CreateNewUserButtonProps {}

const createUserMutation = graphql(`
  mutation AdminCreateUser($input: CreateUserInput!) {
    createUser(input: $input) {
      __typename
      ... on CreateUserSuccess {
        users {
          id
          ...UserAdminPanel_User
        }
      }
      ... on CreateUserError {
        message
      }
    }
  }
`);

export const CreateNewUserButton: React.FC<CreateNewUserButtonProps> = () => {
  const [newUsername, setNewUsername] = useState("");
  const [newPassword, setNewPassword] = useState("");

  const [{ fetching: creating }, createUser] = useMutation(createUserMutation);

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button variant="blue">New User</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create New User</DialogTitle>
          <DialogDescription>
            Provide a unique username and a secure password.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <label className="block text-sm text-gray-300 mb-1">Username</label>
            <Input
              value={newUsername}
              onChange={(e) => setNewUsername(e.target.value)}
              placeholder="e.g. alice"
            />
          </div>
          <div>
            <label className="block text-sm text-gray-300 mb-1">Password</label>
            <Input
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              placeholder="••••••••"
            />
          </div>
        </div>
        <DialogFooter>
          <Button
            variant="blue"
            loading={creating}
            disabled={!newUsername || !newPassword}
            onClick={async () => {
              if (!newUsername || !newPassword) {
                return;
              }
              await createUser({
                input: {
                  username: newUsername,
                  password: newPassword,
                },
              });
              setNewUsername("");
              setNewPassword("");
              (document.activeElement as HTMLElement)?.blur();
            }}
          >
            Create
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
