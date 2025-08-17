import * as React from "react";
import { RoleToggleButton } from "@/components/ui";
import {
  Crown,
  Download,
  Eye,
  KeyRound,
  ListPlus,
  Settings2,
} from "lucide-react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";

export interface UserRolesTogglesProps {
  user: FragmentType<typeof userRolesTogglesUserFragment>;
  canRemoveAdminRole: boolean;
}

const userRolesTogglesUserFragment = graphql(`
  fragment UserRolesToggles_User on User {
    id
    username
    roles
  }
`);

const updateRolesMutation = graphql(`
  mutation UpdateUserRoles($input: UpdateUserRolesInput!) {
    updateUserRoles(input: $input) {
      __typename
      ... on UpdateUserRolesSuccess {
        user {
          id
          ...UserAdminPanel_User
        }
      }
      ... on UpdateUserRolesError {
        message
      }
    }
  }
`);

export const UserRolesToggles: React.FC<UserRolesTogglesProps> = ({
  canRemoveAdminRole,
  ...props
}) => {
  const user = useFragment(userRolesTogglesUserFragment, props.user);

  const [{ fetching: updating }, updateRoles] =
    useMutation(updateRolesMutation);

  const toggleRole = async (bit: number) => {
    const current = (user.roles ?? 0) as number;
    const newRoles = current ^ bit;
    await updateRoles({ input: { userId: user.id, roles: newRoles } });
  };

  return (
    <div className="flex flex-wrap gap-3 ml-auto justify-end">
      <RoleToggleButton
        disabled={updating || !canRemoveAdminRole}
        label="Admin"
        active={((user.roles ?? 0) & (1 << 0)) !== 0}
        onToggle={() => toggleRole(1 << 0)}
        icon={Crown}
      />
      <RoleToggleButton
        disabled={updating}
        label="Create Playlists"
        active={((user.roles ?? 0) & (1 << 1)) !== 0}
        onToggle={() => toggleRole(1 << 1)}
        icon={ListPlus}
      />
      <RoleToggleButton
        disabled={updating}
        label="Trigger Downloads"
        active={((user.roles ?? 0) & (1 << 2)) !== 0}
        onToggle={() => toggleRole(1 << 2)}
        icon={Download}
      />
      <RoleToggleButton
        disabled={updating}
        label="Manage Roles"
        active={((user.roles ?? 0) & (1 << 3)) !== 0}
        onToggle={() => toggleRole(1 << 3)}
        icon={Settings2}
      />
      <RoleToggleButton
        disabled={updating}
        label="View Downloads"
        active={((user.roles ?? 0) & (1 << 4)) !== 0}
        onToggle={() => toggleRole(1 << 4)}
        icon={Eye}
      />
      <RoleToggleButton
        disabled={updating}
        label="Edit External Auth"
        active={((user.roles ?? 0) & (1 << 5)) !== 0}
        onToggle={() => toggleRole(1 << 5)}
        icon={KeyRound}
      />
    </div>
  );
};
