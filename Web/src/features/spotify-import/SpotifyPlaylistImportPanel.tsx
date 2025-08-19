import React, { useState } from "react";
import {
  SpotifyUserInputForm,
  SpotifyUserInputFormModel,
} from "./SpotifyUserInputForm";
import { UserPlaylistsFetcher } from "./UserPlaylistsFetcher.tsx";
import {
  PageLayout,
  PageHeader,
  GlassCard,
  InfoSection,
} from "@/components/ui";
import { Button } from "@/components/ui/button.tsx";
import { Search, Music, Disc3, Download } from "lucide-react";

export const SpotifyPlaylistImportPanel: React.FC = () => {
  const [spotifyUsername, setSpotifyUsername] = useState<string>("");
  const [formModel, setFormModel] = useState<SpotifyUserInputFormModel>({
    username: "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const s = formModel.username.trim();
    if (!s) {
      // Basic validation, can be enhanced
      alert("Please enter a Spotify username or ID.");
      return;
    }
    setSpotifyUsername(s);
  };

  return (
    <PageLayout>
      {/* Beautiful Header */}
      <PageHeader
        icon={Disc3}
        title="Import Spotify Playlists"
        subtitle="Bring your favorite Spotify playlists into your personal music library"
      />

      {/* Main Import Form */}
      <div className="max-w-4xl mx-auto mb-12">
        <GlassCard
          title="Spotify Playlist Import"
          icon={Music}
          iconBgColor="bg-green-500/20"
        >
          <div className="space-y-6">
            <div className="text-center mb-8">
              <div className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-br from-green-500/20 to-emerald-500/20 rounded-2xl border border-green-500/30 mb-4">
                <Disc3 className="w-8 h-8 text-green-400" />
              </div>
              <h3 className="text-xl font-semibold text-white mb-2">
                Connect Your Spotify Account
              </h3>
              <p className="text-gray-300 text-base">
                Enter your Spotify username to discover and import your
                playlists
              </p>
            </div>

            <form onSubmit={handleSubmit} className="space-y-6">
              <SpotifyUserInputForm
                value={formModel}
                onValueChange={setFormModel}
              />

              <div className="flex justify-center">
                <Button
                  type="submit"
                  iconLeft={Search}
                  className="bg-gradient-to-r from-green-500 to-emerald-500 hover:from-green-600 hover:to-emerald-600 text-white px-8 py-3 rounded-xl text-base font-semibold shadow-lg hover:shadow-xl transition-all duration-200"
                >
                  Discover Playlists
                </Button>
              </div>
            </form>
          </div>
        </GlassCard>
      </div>

      {/* Playlists Results */}
      {spotifyUsername && (
        <div className="max-w-6xl mx-auto">
          <UserPlaylistsFetcher spotifyUsername={spotifyUsername} />
        </div>
      )}

      {/* Information Section */}
      <div className="max-w-4xl mx-auto mt-12">
        <InfoSection
          icon={Download}
          title="About Spotify Import"
          variant="green"
        >
          Import your favorite Spotify playlists to build your personal music
          collection. Our system will analyze your playlists and help you
          discover new music while maintaining your carefully curated
          selections. All imported playlists become part of your local library,
          giving you full control over your music experience.
        </InfoSection>
      </div>
    </PageLayout>
  );
};
