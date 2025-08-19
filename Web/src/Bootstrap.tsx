import * as React from "react";
import { PropsWithChildren, ReactNode } from "react";
import { useQuery } from "urql";
import { PageLoading, PageError } from "@/components/ui";
import { Music, AlertTriangle } from "lucide-react";
import { graphql } from "@/gql";

export interface BootstrapProps extends PropsWithChildren {
  renderAuthenticated: () => ReactNode;
  renderNotAuthenticated: () => ReactNode;
  renderNoUsers: () => ReactNode;
}

const bootstrapQuery = graphql(`
  query Bootstrap {
    areThereAnyUsers
    viewer {
      id
    }
  }
`);

export const Bootstrap: React.FC<BootstrapProps> = ({
  renderAuthenticated,
  renderNotAuthenticated,
  renderNoUsers,
}) => {
  const [{ data, fetching, error }] = useQuery({
    query: bootstrapQuery,
  });

  if (fetching) {
    return (
      <PageLoading
        title="Initializing Application"
        subtitle="Checking user status and configuration"
        icon={Music}
        iconBgColor="bg-blue-500/20"
      />
    );
  }

  if (error || !data) {
    return (
      <PageError
        title="Application Error"
        message="We encountered an error while checking user status"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );
  }

  if (!data.areThereAnyUsers) {
    return <>{renderNoUsers()}</>;
  }

  if (data.viewer) {
    return <>{renderAuthenticated()}</>;
  }

  return <>{renderNotAuthenticated()}</>;
};
