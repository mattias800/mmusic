import * as React from "react";
import { useMutation, useQuery } from "urql";
import { graphql } from "@/gql";
import { PageLayout, PageHeader, GlassCard, PageLoading, PageError, PageNoData, RoleToggleButton } from "@/components/ui";
import { Users as UsersIcon, Shield, AlertTriangle, Crown, ListPlus, Download, Settings2, Eye, KeyRound } from "lucide-react";

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
      <div className="max-w-5xl mx-auto space-y-8">
        <GlassCard title="Users" icon={UsersIcon}>
          <div className="divide-y divide-white/10">
            {users.map(u => (
              <div key={u!.id} className="flex items-start justify-between py-4 first:pt-0 last:pb-0">
                <div className="min-w-48">
                  <div className="text-white font-semibold text-base">{u!.username}</div>
                  <div className="text-xs text-gray-400">ID: {u!.id}</div>
                </div>
                <div className="flex flex-wrap gap-3">
                  <RoleToggleButton disabled={updating} label="Admin" active={((u!.roles ?? 0) & (1 << 0)) !== 0} onToggle={() => toggleRole(u!.id, 1 << 0)} icon={Crown} />
                  <RoleToggleButton disabled={updating} label="Create Playlists" active={((u!.roles ?? 0) & (1 << 1)) !== 0} onToggle={() => toggleRole(u!.id, 1 << 1)} icon={ListPlus} />
                  <RoleToggleButton disabled={updating} label="Trigger Downloads" active={((u!.roles ?? 0) & (1 << 2)) !== 0} onToggle={() => toggleRole(u!.id, 1 << 2)} icon={Download} />
                  <RoleToggleButton disabled={updating} label="Manage Roles" active={((u!.roles ?? 0) & (1 << 3)) !== 0} onToggle={() => toggleRole(u!.id, 1 << 3)} icon={Settings2} />
                  <RoleToggleButton disabled={updating} label="View Downloads" active={((u!.roles ?? 0) & (1 << 4)) !== 0} onToggle={() => toggleRole(u!.id, 1 << 4)} icon={Eye} />
                  <RoleToggleButton disabled={updating} label="Edit External Auth" active={((u!.roles ?? 0) & (1 << 5)) !== 0} onToggle={() => toggleRole(u!.id, 1 << 5)} icon={KeyRound} />
                </div>
              </div>
            ))}
          </div>
        </GlassCard>
      </div>
    </PageLayout>
  );
};


