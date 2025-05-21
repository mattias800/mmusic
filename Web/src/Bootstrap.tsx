import * as React from "react";
import { PropsWithChildren, ReactNode } from "react";
import { gql, useQuery } from "urql";
import { ScreenSpinner } from "@/common/components/spinner/ScreenSpinner.tsx";

export interface BootstrapProps extends PropsWithChildren {
  renderAuthenticated: () => ReactNode;
  renderNotAuthenticated: () => ReactNode;
  renderNoUsers: () => ReactNode;
}

const bootstrapQuery = gql`
  query Bootstrap {
    areThereAnyUsers
    viewer {
      id
      username
    }
  }
`;

export const Bootstrap: React.FC<BootstrapProps> = ({
  renderAuthenticated,
  renderNotAuthenticated,
  renderNoUsers,
}) => {
  const [{ data, fetching, error }] = useQuery({
    query: bootstrapQuery,
  });

  if (fetching) {
    return <ScreenSpinner />;
  }

  if (error || !data) {
    return <div>Error checking user status: {error.message}</div>;
  }

  if (!data.areThereAnyUsers) {
    return <>{renderNoUsers()}</>;
  }

  if (data.viewer) {
    return <>{renderAuthenticated()}</>;
  }

  return <>{renderNotAuthenticated()}</>;
};
