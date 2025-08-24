import * as React from "react";
import { AuthTokensStatusPanel } from "@/features/settings/AuthTokensStatusPanel.tsx";
import { SoulSeekConnectionForm } from "@/features/settings/SoulSeekConnectionForm.tsx";
import { ProwlarrSettingsForm } from "@/features/settings/ProwlarrSettingsForm.tsx";
import { QBittorrentSettingsForm } from "@/features/settings/QBittorrentSettingsForm.tsx";
import { SoulSeekReachabilityCard } from "@/features/settings/SoulSeekReachabilityCard.tsx";
import {
  GlassCard,
  PageLayout,
  PageHeader,
  InfoSection,
} from "@/components/ui";
import { PlugZap, Settings, Network } from "lucide-react";

export const AdminIntegrationsTab: React.FC = () => {
  return (
    <PageLayout>
      <PageHeader
        icon={PlugZap}
        title="Integrations"
        subtitle="Manage external service tokens and connections"
      />
      <div className="pt-4 space-y-8 max-w-4xl mx-auto">
        <GlassCard
          title="SoulSeek Connection"
          icon={Settings}
          iconBgColor="bg-purple-500/20"
        >
          <SoulSeekConnectionForm />
        </GlassCard>

        <GlassCard
          title="SoulSeek Reachability"
          icon={Network}
          iconBgColor="bg-blue-500/20"
        >
          <SoulSeekReachabilityCard />
        </GlassCard>

        <GlassCard
          title="Prowlarr"
          icon={Settings}
          iconBgColor="bg-green-500/20"
        >
          <ProwlarrSettingsForm />
        </GlassCard>

        <GlassCard
          title="qBittorrent"
          icon={Settings}
          iconBgColor="bg-yellow-500/20"
        >
          <QBittorrentSettingsForm />
        </GlassCard>

        <GlassCard
          title="Connectivity & Tokens"
          icon={PlugZap}
          iconBgColor="bg-purple-500/20"
        >
          <AuthTokensStatusPanel />
        </GlassCard>
      </div>
      <InfoSection icon={PlugZap} title="About Integrations" variant="purple">
        Configure connectivity and credentials for external services used by
        mmusic.
      </InfoSection>
    </PageLayout>
  );
};
