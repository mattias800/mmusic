import * as React from "react";
import { useQuery } from "urql";
import { LibraryPathForm } from "@/features/settings/LibraryPathForm.tsx";
import { DownloadSlotSettingsForm } from "@/features/settings/DownloadSlotSettingsForm.tsx";
import { TopTracksServiceSettingsForm } from "@/features/settings/TopTracksServiceSettingsForm.tsx";
import {
  GlassCard,
  InfoSection,
  PageError,
  PageHeader,
  PageLayout,
  PageLoading,
  PageNoData,
} from "@/components/ui";
import { AlertTriangle, Cog, Settings } from "lucide-react";
import { PublicBaseUrlForm } from "@/features/settings/PublicBaseUrlForm.tsx";
import { graphql } from "@/gql";
import { SoulSeekConnectionForm } from "@/features/settings/SoulSeekConnectionForm.tsx";

export interface SettingsPageProps {}

const settingsPageQuery = graphql(`
  query SettingsPage {
    serverSettings {
      id
      ...LibraryPathForm_ServerSettings
      libraryPath
      downloadSlotCount
      listenBrainzTopTracksEnabled
      spotifyTopTracksEnabled
      lastFmTopTracksEnabled
      publicBaseUrl
    }
  }
`);

export const SettingsPage: React.FC<SettingsPageProps> = () => {
  const [{ error, data, fetching }] = useQuery({
    query: settingsPageQuery,
    requestPolicy: "network-only",
  });

  if (fetching)
    return (
      <PageLoading
        title="Loading Settings"
        subtitle="Fetching your server configuration"
        icon={Settings}
        iconBgColor="bg-blue-500/20"
      />
    );

  if (error)
    return (
      <PageError
        title="Failed to Load Settings"
        message="We couldn't load your server settings"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );

  if (!data?.serverSettings)
    return (
      <PageNoData
        title="Settings Not Available"
        message="Your server settings couldn't be loaded"
        icon={Cog}
        iconBgColor="bg-yellow-500/20"
      />
    );

  return (
    <PageLayout>
      {/* Header Section */}
      <PageHeader
        icon={Settings}
        title="Server Settings"
        subtitle="Configure your mmusic server and download settings"
      />

      {/* Main Content Grid */}
      <div className="max-w-4xl mx-auto space-y-8">
        {/* Library Path Settings */}
        <GlassCard
          title="Library Path Configuration"
          icon={Settings}
          iconBgColor="bg-blue-500/20"
        >
          <LibraryPathForm serverSettings={data.serverSettings} />
        </GlassCard>

        {/* Download Settings */}
        <GlassCard
          title="Download Configuration"
          icon={Settings}
          iconBgColor="bg-green-500/20"
        >
          <DownloadSlotSettingsForm serverSettings={data.serverSettings} />
        </GlassCard>

        {/* Top Tracks Settings */}
        <GlassCard
          title="Top Tracks Integration"
          icon={Settings}
          iconBgColor="bg-purple-500/20"
        >
          <TopTracksServiceSettingsForm />
        </GlassCard>

        {/* SoulSeek Connection Settings */}
        <GlassCard
          title="SoulSeek Connection"
          icon={Settings}
          iconBgColor="bg-purple-500/20"
        >
          <SoulSeekConnectionForm />
        </GlassCard>

        {/* Public Base URL for Casting */}
        <GlassCard
          title="Public Base URL"
          icon={Settings}
          iconBgColor="bg-blue-500/20"
        >
          <PublicBaseUrlForm serverSettings={data.serverSettings} />
        </GlassCard>
      </div>

      {/* Bottom Info Section */}
      <InfoSection icon={Settings} title="About Server Settings" variant="blue">
        These settings control how your mmusic server operates. The library path
        determines where your music files are stored, download slots control how
        many simultaneous downloads can run, and top tracks integration enables
        external services to provide music recommendations. Changes to these
        settings may require a server restart to take effect.
      </InfoSection>
    </PageLayout>
  );
};
