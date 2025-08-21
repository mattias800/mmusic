import * as React from "react";
import { useQuery } from "urql";
import { graphql } from "@/gql";
import { Download, Settings } from "lucide-react";
import { GlassCard, PageLayout, PageHeader, PageLoading, PageError, PageNoData } from "@/components/ui";
import { DownloadSlotSettingsForm } from "@/features/settings/DownloadSlotSettingsForm.tsx";
import { DownloadersTogglesCard } from "@/features/settings/DownloadersTogglesCard.tsx";
import { AlertTriangle } from "lucide-react";

const adminDownloadsQuery = graphql(`
  query AdminDownloadsSettings {
    serverSettings {
      downloadSlotCount
    }
  }
`);

export const AdminDownloadsTab: React.FC = () => {
  const [{ data, fetching, error }] = useQuery({ query: adminDownloadsQuery });

  if (fetching)
    return (
      <PageLoading
        title="Loading Downloads Settings"
        subtitle="Fetching current download configuration"
        icon={Download}
        iconBgColor="bg-blue-500/20"
      />
    );

  if (error)
    return (
      <PageError
        title="Failed to Load Downloads Settings"
        message="We couldn't load downloads configuration"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );

  if (!data?.serverSettings)
    return (
      <PageNoData
        title="No Downloads Settings"
        message="Downloads configuration couldn't be loaded"
        icon={Download}
        iconBgColor="bg-yellow-500/20"
      />
    );

  return (
    <PageLayout>
      <PageHeader
        icon={Download}
        title="Downloads"
        subtitle="Configure concurrent downloads and behavior"
      />
      <div className="pt-4 max-w-4xl mx-auto space-y-8">
        <DownloadersTogglesCard />
        <GlassCard
          title="Download Configuration"
          icon={Settings}
          iconBgColor="bg-green-500/20"
        >
          <DownloadSlotSettingsForm serverSettings={data.serverSettings} />
        </GlassCard>
      </div>
    </PageLayout>
  );
};


