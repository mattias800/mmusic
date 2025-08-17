import * as React from "react";
import { useMutation, useQuery } from "urql";
import { graphql } from "@/gql";
import { PageLayout, PageHeader, GlassCard, PageLoading, PageError, PageNoData } from "@/components/ui";
import { Users as UsersIcon, Shield, AlertTriangle } from "lucide-react";

const usersQuery = graphql(`
  query AdminUsersPage {
    user {
      users {
        nodes {
          id
          username
          roles
        }
      }
    }
    viewer { id roles }
  }
`);

const updateRolesMutation = graphql(`
  mutation UpdateUserRoles($input: UpdateUserRolesInput!) {
    updateUserRoles(input: $input) {
      __typename
      ... on UpdateUserRolesSuccess { user { id username roles } }
      ... on UpdateUserRolesError { message }
    }
  }
`);

export const AdminUsersPage: React.FC = () => {
  const [{ error, data, fetching, stale }] = useQuery({ query: usersQuery });
  const [{ fetching: updating }, updateRoles] = useMutation(updateRolesMutation);

  const isAdmin = (data?.viewer?.roles ?? 0) & 1; // Admin flag bit 0

  if (fetching || stale) return <PageLoading title="Loading Users" subtitle="Fetching users" icon={UsersIcon} iconBgColor="bg-blue-500/20" />;
  if (error) return <PageError title="Failed to Load Users" message="We couldn't load users" error={error} icon={AlertTriangle} iconBgColor="bg-red-500/20" />;
  if (!isAdmin) return <PageNoData title="Not authorized" message="You need admin role to view this page" icon={Shield} iconBgColor="bg-yellow-500/20"/>;

  const users = data?.user?.users?.nodes ?? [];

  const toggleRole = async (userId: string, bit: number) => {
    const user = users.find(u => u?.id === userId);
    if (!user) return;
    const current = (user.roles ?? 0) as number;
    const newRoles = current ^ bit;
    await updateRoles({ input: { userId, roles: newRoles } });
  };

  return (
    <PageLayout>
      <PageHeader icon={UsersIcon} title="User Administration" subtitle="Manage user roles and permissions" />
      <div className="max-w-4xl mx-auto space-y-6">
        <GlassCard title="Users" icon={UsersIcon}>
          <div className="space-y-4">
            {users.map(u => (
              <div key={u!.id} className="flex items-center justify-between p-3 rounded-lg bg-white/5 border border-white/10">
                <div className="text-white font-medium">{u!.username}</div>
                <div className="flex gap-2">
                  <button disabled={updating} onClick={() => toggleRole(u!.id, 1 << 0)} className="px-3 py-1 rounded bg-blue-500/20 hover:bg-blue-500/30 text-blue-200">Admin</button>
                  <button disabled={updating} onClick={() => toggleRole(u!.id, 1 << 1)} className="px-3 py-1 rounded bg-blue-500/20 hover:bg-blue-500/30 text-blue-200">Create Playlists</button>
                  <button disabled={updating} onClick={() => toggleRole(u!.id, 1 << 2)} className="px-3 py-1 rounded bg-blue-500/20 hover:bg-blue-500/30 text-blue-200">Trigger Downloads</button>
                  <button disabled={updating} onClick={() => toggleRole(u!.id, 1 << 3)} className="px-3 py-1 rounded bg-blue-500/20 hover:bg-blue-500/30 text-blue-200">Manage Roles</button>
                  <button disabled={updating} onClick={() => toggleRole(u!.id, 1 << 4)} className="px-3 py-1 rounded bg-blue-500/20 hover:bg-blue-500/30 text-blue-200">View Downloads</button>
                  <button disabled={updating} onClick={() => toggleRole(u!.id, 1 << 5)} className="px-3 py-1 rounded bg-blue-500/20 hover:bg-blue-500/30 text-blue-200">Edit External Auth</button>
                </div>
              </div>
            ))}
          </div>
        </GlassCard>
      </div>
    </PageLayout>
  );
};


