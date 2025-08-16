import React from "react";
import { useQuery } from "urql";
import { SpotifyPlaylistsList } from "./SpotifyPlaylistsList.tsx";
import { graphql } from "@/gql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { MessageBox } from "@/components/errors/MessageBox.tsx";
import { GlassCard } from "@/components/ui";
import { Button } from "@/components/ui/button.tsx";
import { RefreshCw, AlertCircle, Music } from "lucide-react";

interface UserPlaylistsFetcherProps {
  spotifyUsername: string;
}

const userPlaylistsFetcherQuery = graphql(`
  query UserPlaylistsLoader_Query($spotifyUsername: String!) {
    playlist {
      importPlaylists {
        spotify {
          spotifyPlaylistsForUser(username: $spotifyUsername) {
            id
            ...SpotifyPlaylistsList_SpotifyPlaylist
          }
        }
      }
    }
  }
`);

export const UserPlaylistsFetcher: React.FC<UserPlaylistsFetcherProps> = ({
  spotifyUsername,
}) => {
  const [{ error, data, fetching }, executeFetchUserPlaylists] = useQuery({
    query: userPlaylistsFetcherQuery,
    variables: { spotifyUsername },
    requestPolicy: "network-only",
  });

  if (fetching) {
    return (
      <GlassCard 
        title="Loading Playlists" 
        icon={Music}
        iconBgColor="bg-blue-500/20"
      >
        <div className="text-center py-12">
          <SpinnerSpacing>
            <Spinner />
          </SpinnerSpacing>
          <p className="text-gray-300 mt-4">Discovering your Spotify playlists...</p>
        </div>
      </GlassCard>
    );
  }

  if (error) {
    return (
      <GlassCard 
        title="Error Loading Playlists" 
        icon={AlertCircle}
        iconBgColor="bg-red-500/20"
      >
        <div className="text-center py-8">
          <MessageBox message={error.message}>
            <div className="mt-6">
              <Button
                variant={"secondary"}
                onClick={() =>
                  executeFetchUserPlaylists({ requestPolicy: "network-only" })
                }
                className="bg-gradient-to-r from-red-500/20 to-pink-500/20 border-red-500/30 text-red-300 hover:from-red-500/30 hover:to-pink-500/30"
              >
                <RefreshCw className="w-4 h-4 mr-2" />
                Retry
              </Button>
            </div>
          </MessageBox>
        </div>
      </GlassCard>
    );
  }

  const playlists =
    data?.playlist?.importPlaylists?.spotify?.spotifyPlaylistsForUser;

  if (!playlists) {
    return (
      <GlassCard 
        title="Something Went Wrong" 
        icon={AlertCircle}
        iconBgColor="bg-red-500/20"
      >
        <div className="text-center py-8">
          <MessageBox message={"Something went wrong."}>
            <div className="mt-6">
              <Button
                variant={"secondary"}
                onClick={() =>
                  executeFetchUserPlaylists({ requestPolicy: "network-only" })
                }
                className="bg-gradient-to-r from-red-500/20 to-pink-500/20 border-red-500/30 text-red-300 hover:from-red-500/30 hover:to-pink-500/30"
              >
                <RefreshCw className="w-4 h-4 mr-2" />
                Retry
              </Button>
            </div>
          </MessageBox>
        </div>
      </GlassCard>
    );
  }

  if (playlists.length === 0) {
    return (
      <GlassCard 
        title="No Playlists Found" 
        icon={Music}
        iconBgColor="bg-yellow-500/20"
      >
        <div className="text-center py-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-br from-yellow-500/20 to-orange-500/20 rounded-2xl border border-yellow-500/30 mb-4">
            <Music className="w-8 h-8 text-yellow-400" />
          </div>
          <h3 className="text-xl font-semibold text-white mb-2">
            No Playlists Found
          </h3>
          <p className="text-gray-300 mb-4">
            No playlists found for user: <span className="font-semibold text-white">{spotifyUsername}</span>
          </p>
          <p className="text-gray-400 text-sm">
            Make sure the username is correct and the account has public playlists
          </p>
        </div>
      </GlassCard>
    );
  }

  return (
    <GlassCard 
      title={`${spotifyUsername}'s Playlists`}
      icon={Music}
      iconBgColor="bg-green-500/20"
    >
      <div className="space-y-6">
        <div className="text-center mb-6">
          <div className="inline-flex items-center justify-center w-12 h-12 bg-gradient-to-br from-green-500/20 to-emerald-500/20 rounded-xl border border-green-500/30 mb-3">
            <Music className="w-6 h-6 text-green-400" />
          </div>
          <p className="text-gray-300">
            Found <span className="font-semibold text-white">{playlists.length}</span> playlists to import
          </p>
        </div>
        
        <SpotifyPlaylistsList
          playlists={playlists}
          spotifyUsername={spotifyUsername}
        />
      </div>
    </GlassCard>
  );
};
