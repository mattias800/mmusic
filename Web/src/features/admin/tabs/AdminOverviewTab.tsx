import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { ServerLibraryStatisticsHeader } from "@/features/server-library/ServerLibraryStatisticsHeader.tsx";
import {
  PageError,
  PageHeader,
  PageLayout,
  PageLoading,
  PageNoData,
} from "@/components/ui";
import { AlertTriangle, Gauge } from "lucide-react";
import { SoulSeekReachabilitySummary } from "@/features/soul-seek-network-status/SoulSeekReachabilitySummary.tsx";
import { DiskUsagePanel } from "@/features/storage-statistics/DiskUsagePanel.tsx";

const adminOverviewQuery = graphql(`
  query AdminOverviewTabQuery {
    serverLibrary {
      ...ServerLibraryStatisticsHeader_ServerLibrary
    }
    serverSettings {
      storageStats {
        ...DiskUsagePanel_StorageStats
      }
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
      <PageHeader
        icon={Gauge}
        title="Overview"
        subtitle="Server library status"
      />
      <div className="pt-4 space-y-4">
        <ServerLibraryStatisticsHeader serverLibrary={data.serverLibrary} />
        {data.serverSettings.storageStats && (
          <DiskUsagePanel storageStats={data.serverSettings.storageStats} />
        )}
        <SoulSeekReachabilitySummary />
      </div>
    </PageLayout>
  );
};
