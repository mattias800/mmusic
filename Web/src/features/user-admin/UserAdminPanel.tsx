import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { UserAdminCard } from "@/features/user-admin/UserAdminCard.tsx";
import { CreateNewUserButton } from "@/features/user-admin/CreateNewUserButton.tsx";
import { sumBy } from "lodash-es";
import { UserRolesToggles } from "@/features/user-admin/UserRolesToggles.tsx";
import { UserDeleteButton } from "@/features/user-admin/UserDeleteButton.tsx";
import { UserChangeUsernameButton } from "@/features/user-admin/UserChangeUsernameButton.tsx";
import { UserChangePasswordButton } from "@/features/user-admin/UserChangePasswordButton.tsx";

export interface UserAdminPanelProps {
  viewer: FragmentType<typeof userAdminPanelViewerFragment>;
  users: Array<FragmentType<typeof userAdminPanelUserFragment>>;
  onUsersChanged?: () => void;
}

const userAdminPanelViewerFragment = graphql(`
  fragment UserAdminPanel_Viewer on User {
    id
    roles
    username
    createdAt
  }
`);

const userAdminPanelUserFragment = graphql(`
  fragment UserAdminPanel_User on User {
    id
    ...UserDeleteButton_User
    ...UserChangeUsernameButton_User
    ...UserChangePasswordButton_User
    ...UserRolesToggles_User
    ...UserAdminCard_User
    roles
    username
    createdAt
    isAdmin
  }
`);

export const UserAdminPanel: React.FC<UserAdminPanelProps> = (props) => {
  const users = useFragment(userAdminPanelUserFragment, props.users);
  const viewer = useFragment(userAdminPanelViewerFragment, props.viewer);

  const numAdmins = sumBy(users, (u) => (u.isAdmin ? 1 : 0));

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-6">
        {users.map((user) => (
          <UserAdminCard
            key={user.id}
            user={user}
            renderButtons={() => (
              <div className="flex flex-col items-center gap-3">
                <UserChangeUsernameButton user={user} />
                <UserChangePasswordButton user={user} />
                <UserDeleteButton user={user} />
              </div>
            )}
            renderRoleToggles={() => (
              <UserRolesToggles
                user={user}
                canRemoveAdminRole={numAdmins > 1 && user.id !== viewer.id}
              />
            )}
          />
        ))}
      </div>
      <div>
        <CreateNewUserButton />
      </div>
    </div>
  );
};
