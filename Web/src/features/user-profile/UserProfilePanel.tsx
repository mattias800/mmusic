import { FragmentType, graphql, useFragment } from "@/gql";
import React from "react";
import { SignOutButton } from "@/features/auth/components/SignOutButton.tsx";
import { ListenBrainzSettingsForm } from "@/features/user/components/ListenBrainzSettingsForm";
import { User, Calendar, Shield, Settings, Music } from "lucide-react";
import { 
  PageLayout, 
  PageHeader, 
  GlassCard, 
  StatusCard, 
  StatusGrid, 
  InfoSection 
} from "@/components/ui";

export interface UserProfilePanelProps {
  user: FragmentType<typeof userProfilePanelUserFragment>;
}

const userProfilePanelUserFragment = graphql(`
  fragment UserProfilePanel_User on User {
    id
    username
    createdAt
    updatedAt
    listenBrainzUserId
    hasListenBrainzToken
  }
`);

export const UserProfilePanel: React.FC<UserProfilePanelProps> = (props) => {
  const user = useFragment(userProfilePanelUserFragment, props.user);

  const { username, createdAt, updatedAt, listenBrainzUserId, hasListenBrainzToken } = user;

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  return (
    <PageLayout>
      {/* Header Section */}
      <PageHeader 
        icon={User} 
        title={username} 
        subtitle="User Profile" 
      />

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-8 max-w-7xl mx-auto">
        {/* Left Column - Account Information */}
        <div className="xl:col-span-1">
          <GlassCard 
            title="Account Information" 
            icon={User} 
            iconBgColor="bg-blue-500/20"
          >
            <div className="space-y-4 mb-8">
              <StatusCard label="Username" value={username} />
              <StatusCard label="Joined" value={formatDate(createdAt)} icon={Calendar} />
              <StatusCard label="Updated" value={formatDate(updatedAt)} icon={Calendar} />
            </div>

            <div className="border-t border-white/10 pt-6">
              <SignOutButton />
            </div>
          </GlassCard>
        </div>

        {/* Right Column - ListenBrainz Settings */}
        <div className="xl:col-span-2">
          <GlassCard 
            title="ListenBrainz Integration" 
            icon={Music} 
            iconBgColor="bg-green-500/20"
          >
            {/* Current Status */}
            <div className="mb-8 p-6 bg-gradient-to-r from-white/5 to-white/10 rounded-xl border border-white/10">
              <h3 className="text-lg font-semibold text-white mb-4 flex items-center gap-2">
                <Shield className="w-5 h-5 text-blue-400" />
                Connection Status
              </h3>
              <StatusGrid columns={2}>
                <StatusCard 
                  label="Username" 
                  value={listenBrainzUserId || 'Not configured'} 
                />
                <StatusCard 
                  label="API Token" 
                  value={
                    hasListenBrainzToken ? (
                      <span className="flex items-center gap-2">
                        <span className="w-2 h-2 bg-green-400 rounded-full"></span>
                        Connected
                      </span>
                    ) : (
                      <span className="flex items-center gap-2">
                        <span className="w-2 h-2 bg-gray-400 rounded-full"></span>
                        Not configured
                      </span>
                    )
                  } 
                />
              </StatusGrid>
            </div>

            {/* Settings Form */}
            <ListenBrainzSettingsForm
              userId={user.id}
              currentListenBrainzUserId={listenBrainzUserId}
              hasListenBrainzToken={hasListenBrainzToken}
            />
          </GlassCard>
        </div>
      </div>

      {/* Bottom Info Section */}
      <InfoSection 
        icon={Settings} 
        title="About ListenBrainz" 
        variant="blue"
      >
        ListenBrainz is an open source alternative to Last.fm that tracks your music listening history. 
        By connecting your account, mmusic will automatically report your listening activity, helping you 
        discover new music and contribute to the open music data ecosystem.
      </InfoSection>
    </PageLayout>
  );
}
