import * as React from "react";
import { useQuery } from "urql";
import { PageError, PageLoading } from "@/components/ui";
import { DownloadersTogglesCardPanel } from "@/features/settings/DownloadersTogglesCardPanel.tsx";
import { AlertTriangle, Download } from "lucide-react";
import { downloadersTogglesCardQuery } from "@/features/settings/DownloadersTogglesCardMutations.tsx";

export const DownloadersTogglesCard: React.FC = () => {
  const [{ data, fetching, error }] = useQuery({
    query: downloadersTogglesCardQuery,
  });

  if (fetching) {
    return (
      <PageLoading
        title="Loading Downloaders"
        subtitle="Fetching current downloader settings"
        icon={Download}
        iconBgColor="bg-blue-500/20"
      />
    );
  }

  if (error || !data?.serverSettings) {
    return (
      <PageError
        title="Failed to Load Downloaders"
        message="We couldn't load downloader settings"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );
  }

  return <DownloadersTogglesCardPanel serverSettings={data.serverSettings} />;
};
