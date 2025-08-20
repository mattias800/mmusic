import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { ServerLibraryStatisticsHeader } from "@/features/server-library/ServerLibraryStatisticsHeader.tsx";
import {
  PageLayout,
  PageHeader,
  PageLoading,
  PageError,
  PageNoData,
} from "@/components/ui";
import { Gauge, AlertTriangle } from "lucide-react";

const adminOverviewQuery = graphql(`
  query AdminOverviewTabQuery {
    serverLibrary {
      ...ServerLibraryStatisticsHeader_ServerLibrary
    }
  }
`);

export const AdminOverviewTab: React.FC = () => {
  const [{ data, fetching, error }] = useQuery({ query: adminOverviewQuery });

  if (fetching)
    return (
      <PageLoading
        title="Loading Overview"
        subtitle="Fetching server library overview"
        icon={Gauge}
        iconBgColor="bg-blue-500/20"
      />
    );

  if (error)
    return (
      <PageError
        title="Failed to Load Overview"
        message="We couldn't load the overview"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );

  if (!data?.serverLibrary)
    return (
      <PageNoData
        title="No Overview Data"
        message="Server library data couldn't be loaded"
        icon={Gauge}
        iconBgColor="bg-yellow-500/20"
      />
    );

  return (
    <PageLayout>
      <PageHeader icon={Gauge} title="Overview" subtitle="Server library status" />
      <div className="pt-4">
        <ServerLibraryStatisticsHeader serverLibrary={data.serverLibrary} />
      </div>
    </PageLayout>
  );
};


