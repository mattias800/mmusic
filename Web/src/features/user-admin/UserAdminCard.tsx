import * as React from "react";
import { ReactNode } from "react";
import { User } from "lucide-react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { GlassCardInner } from "@/components/ui/glas-card-inner.tsx";

export interface UserAdminCardProps {
  user: FragmentType<typeof userAdminCardUserFragment>;
  renderRoleToggles: () => ReactNode;
  renderButtons: () => ReactNode;
}

const userAdminCardUserFragment = graphql(`
  fragment UserAdminCard_User on User {
    id
    username
    roles
  }
`);

export const UserAdminCard: React.FC<UserAdminCardProps> = ({
  renderButtons,
  renderRoleToggles,
  ...props
}) => {
  const user = useFragment(userAdminCardUserFragment, props.user);

  return (
    <GlassCardInner
      key={user.id}
      heading={user.username}
      icon={<User className="w-5 h-5 text-purple-400" />}
    >
      <div className="flex items-start justify-between gap-4">
        <div className="min-w-56 flex flex-col gap-4">
          <div className="flex items-center gap-3">{renderButtons()}</div>
          <div className="text-xs text-gray-400">ID: {user.id}</div>
        </div>
        {renderRoleToggles()}
      </div>
    </GlassCardInner>
  );
};
