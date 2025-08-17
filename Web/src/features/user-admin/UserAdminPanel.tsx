import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { GlassCard, PageHeader, PageLayout } from "@/components/ui";
import { Users as UsersIcon } from "lucide-react";
import { UserAdminCard } from "@/features/user-admin/UserAdminCard.tsx";
import { CreateNewUserButton } from "@/features/user-admin/CreateNewUserButton.tsx";
import { sumBy } from "lodash-es";
import { UserEditButton } from "@/features/user-admin/UserEditButton.tsx";
import { UserRolesToggles } from "@/features/user-admin/UserRolesToggles.tsx";

export interface UserAdminPanelProps {
  viewer: FragmentType<typeof userAdminPanelViewerFragment>;
  users: Array<FragmentType<typeof userAdminPanelUserFragment>>;
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
    ...UserEditButton_User
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
    <PageLayout>
      <PageHeader
        icon={UsersIcon}
        title="User Administration"
        subtitle="Manage user roles and permissions"
      />
      <div className="flex flex-col gap-8">
        <GlassCard title="Users" icon={UsersIcon}>
          <div className="flex flex-col gap-8">
            {users.map((user) => (
              <UserAdminCard
                key={user.id}
                user={user}
                renderEditButton={() => <UserEditButton user={user} />}
                renderRoleToggles={() => (
                  <UserRolesToggles
                    user={user}
                    canRemoveAdminRole={numAdmins > 1 && user.id !== viewer.id}
                  />
                )}
              />
            ))}
            <div className="flex justify-start"></div>
          </div>
          <CreateNewUserButton />
        </GlassCard>
      </div>
    </PageLayout>
  );
};
