import * as React from "react";
import { useQuery } from "urql";
import { graphql } from "@/gql";
import { PageError, PageLoading, PageNoData } from "@/components/ui";
import { AlertTriangle, Shield, Users as UsersIcon } from "lucide-react";
import { UserAdminPanel } from "@/features/user-admin/UserAdminPanel.tsx";

const usersQuery = graphql(`
  query AdminUsersPage {
    user {
      users {
        nodes {
          ...UserAdminPanel_User
        }
      }
    }
    viewer {
      id
      ...UserAdminPanel_Viewer
      roles
      isAdmin
    }
  }
`);

export const AdminUsersPage: React.FC = () => {
  const [{ error, data, fetching, stale }] = useQuery({ query: usersQuery });

  if (fetching || stale)
    return (
      <PageLoading
        title="Loading Users"
        subtitle="Fetching users"
        icon={UsersIcon}
        iconBgColor="bg-blue-500/20"
      />
    );

  if (error || !data)
    return (
      <PageError
        title="Failed to Load Users"
        message="We couldn't load users"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );

  if (!data.viewer || !data.viewer?.isAdmin)
    return (
      <PageNoData
        title="Not authorized"
        message="You need admin role to view this page"
        icon={Shield}
        iconBgColor="bg-yellow-500/20"
      />
    );

  return (
    <UserAdminPanel users={data.user.users?.nodes ?? []} viewer={data.viewer} />
  );
};
