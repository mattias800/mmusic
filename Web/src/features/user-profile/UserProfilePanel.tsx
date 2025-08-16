import { FragmentType, graphql, useFragment } from "@/gql";
import React from "react";
import { SignOutButton } from "@/features/auth/components/SignOutButton.tsx";
import { ListenBrainzSettingsForm } from "@/features/user/components/ListenBrainzSettingsForm";
import { User, Calendar, Shield, Settings, Music } from "lucide-react";

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
    <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900">
      <div className="container mx-auto px-4 py-8">
        {/* Header Section */}
        <div className="text-center mb-12">
          <div className="inline-flex items-center justify-center w-24 h-24 bg-gradient-to-br from-blue-500 via-purple-500 to-pink-500 rounded-full mb-6 shadow-2xl">
            <User className="w-12 h-12 text-white" />
          </div>
          <h1 className="text-4xl font-bold text-white mb-2">{username}</h1>
          <p className="text-gray-300 text-lg">User Profile</p>
        </div>

        {/* Main Content Grid */}
        <div className="grid grid-cols-1 xl:grid-cols-3 gap-8 max-w-7xl mx-auto">
          {/* Left Column - Account Information */}
          <div className="xl:col-span-1">
            <div className="bg-white/5 backdrop-blur-sm border border-white/10 rounded-2xl p-8 shadow-2xl">
              <div className="flex items-center gap-3 mb-6">
                <div className="p-2 bg-blue-500/20 rounded-lg">
                  <User className="w-5 h-5 text-blue-400" />
                </div>
                <h2 className="text-xl font-semibold text-white">Account Information</h2>
              </div>
              
              <div className="space-y-4 mb-8">
                <div className="flex items-center gap-3 p-3 bg-white/5 rounded-lg">
                  <span className="text-gray-400 text-sm font-medium min-w-[80px]">Username</span>
                  <span className="text-white font-medium">{username}</span>
                </div>
                
                <div className="flex items-center gap-3 p-3 bg-white/5 rounded-lg">
                  <Calendar className="w-4 h-4 text-gray-400" />
                  <span className="text-gray-400 text-sm font-medium min-w-[80px]">Joined</span>
                  <span className="text-white">{formatDate(createdAt)}</span>
                </div>
                
                <div className="flex items-center gap-3 p-3 bg-white/5 rounded-lg">
                  <Calendar className="w-4 h-4 text-gray-400" />
                  <span className="text-gray-400 text-sm font-medium min-w-[80px]">Updated</span>
                  <span className="text-white">{formatDate(updatedAt)}</span>
                </div>
              </div>

              <div className="border-t border-white/10 pt-6">
                <SignOutButton />
              </div>
            </div>
          </div>

          {/* Right Column - ListenBrainz Settings */}
          <div className="xl:col-span-2">
            <div className="bg-white/5 backdrop-blur-sm border border-white/10 rounded-2xl p-8 shadow-2xl">
              <div className="flex items-center gap-3 mb-6">
                <div className="p-2 bg-green-500/20 rounded-lg">
                  <Music className="w-5 h-5 text-green-400" />
                </div>
                <h2 className="text-xl font-semibold text-white">ListenBrainz Integration</h2>
              </div>

              {/* Current Status */}
              <div className="mb-8 p-6 bg-gradient-to-r from-white/5 to-white/10 rounded-xl border border-white/10">
                <h3 className="text-lg font-semibold text-white mb-4 flex items-center gap-2">
                  <Shield className="w-5 h-5 text-blue-400" />
                  Connection Status
                </h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="p-4 bg-white/5 rounded-lg">
                    <span className="text-gray-400 text-sm font-medium">Username</span>
                    <div className="text-white font-medium mt-1">
                      {listenBrainzUserId || 'Not configured'}
                    </div>
                  </div>
                  <div className="p-4 bg-white/5 rounded-lg">
                    <span className="text-gray-400 text-sm font-medium">API Token</span>
                    <div className="text-white font-medium mt-1">
                      {hasListenBrainzToken ? (
                        <span className="flex items-center gap-2">
                          <span className="w-2 h-2 bg-green-400 rounded-full"></span>
                          Connected
                        </span>
                      ) : (
                        <span className="flex items-center gap-2">
                          <span className="w-2 h-2 bg-gray-400 rounded-full"></span>
                          Not configured
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              {/* Settings Form */}
              <ListenBrainzSettingsForm
                userId={user.id}
                currentListenBrainzUserId={listenBrainzUserId}
                hasListenBrainzToken={hasListenBrainzToken}
              />
            </div>
          </div>
        </div>

        {/* Bottom Info Section */}
        <div className="mt-12 max-w-4xl mx-auto">
          <div className="bg-gradient-to-r from-blue-500/10 to-purple-500/10 border border-blue-500/20 rounded-2xl p-8 text-center">
            <div className="flex items-center justify-center gap-3 mb-4">
              <Settings className="w-6 h-6 text-blue-400" />
              <h3 className="text-xl font-semibold text-white">About ListenBrainz</h3>
            </div>
            <p className="text-gray-300 leading-relaxed">
              ListenBrainz is an open source alternative to Last.fm that tracks your music listening history. 
              By connecting your account, mmusic will automatically report your listening activity, helping you 
              discover new music and contribute to the open music data ecosystem.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
