import * as React from "react";
import { useQuery } from "urql";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { graphql } from "@/gql";
import { LibraryPathForm } from "@/features/settings/LibraryPathForm.tsx";
import { SoulSeekSettingsForm } from "@/features/settings/SoulSeekSettingsForm.tsx";
import { DownloadSlotSettingsForm } from "@/features/settings/DownloadSlotSettingsForm.tsx";
import { PageLayout, PageHeader, GlassCard, InfoSection } from "@/components/ui";
import { Settings, Download, FolderOpen, Server, Info } from "lucide-react";

export interface SettingsPageProps {}

const settingsPageQuery = graphql(`
  query SettingsPage {
    serverSettings {
      ...LibraryPathForm_ServerSettings
      downloadSlotCount
    }
  }
`);

export const SettingsPage: React.FC<SettingsPageProps> = () => {
  const [{ error, data, fetching, stale }] = useQuery({
    query: settingsPageQuery,
  });
  
  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.serverSettings) return <div>No data</div>;

  return (
    <PageLayout>
      {/* Header Section */}
      <PageHeader 
        icon={Settings} 
        title="Server Settings" 
        subtitle="Configure your mmusic server and download settings" 
      />

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-8 max-w-7xl mx-auto">
        {/* Left Column - Library & Storage */}
        <div className="space-y-8">
          <GlassCard 
            title="Library Configuration" 
            icon={FolderOpen} 
            iconBgColor="bg-blue-500/20"
          >
            <LibraryPathForm serverSettings={data.serverSettings} />
          </GlassCard>
        </div>

        {/* Right Column - Download & External Services */}
        <div className="space-y-8">
          <GlassCard 
            title="Download Settings" 
            icon={Download} 
            iconBgColor="bg-green-500/20"
          >
            <DownloadSlotSettingsForm serverSettings={data.serverSettings} />
          </GlassCard>

          <GlassCard 
            title="External Services" 
            icon={Server} 
            iconBgColor="bg-purple-500/20"
          >
            <SoulSeekSettingsForm />
          </GlassCard>
        </div>
      </div>

      {/* Bottom Info Section */}
      <InfoSection 
        icon={Info} 
        title="About Server Settings" 
        variant="blue"
      >
        These settings control how mmusic operates on your server. The library path determines where your music files are stored, 
        download slots control how many releases can be downloaded simultaneously, and external service settings configure 
        how mmusic interacts with services like SoulSeek. Changes to these settings may require a server restart to take effect.
      </InfoSection>
    </PageLayout>
  );
};
