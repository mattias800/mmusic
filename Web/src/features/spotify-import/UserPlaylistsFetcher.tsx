import React from "react";
import { useQuery } from "urql";
import { SpotifyPlaylistsList } from "./SpotifyPlaylistsList.tsx";
import { graphql } from "@/gql";
import { PageLoading, PageError, PageNoData } from "@/components/ui";
import { Music, AlertTriangle, Heart } from "lucide-react";

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
      <PageLoading
        title="Loading Spotify Playlists"
        subtitle="Discovering your Spotify playlists..."
        icon={Music}
        iconBgColor="bg-blue-500/20"
      />
    );
  }

  if (error) {
    return (
      <PageError
        title="Failed to Load Playlists"
        message="We couldn't load your Spotify playlists"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
        onRetry={() =>
          executeFetchUserPlaylists({ requestPolicy: "network-only" })
        }
        retryText="Try Again"
      />
    );
  }

  const playlists =
    data?.playlist?.importPlaylists?.spotify?.spotifyPlaylistsForUser;

  if (!playlists) {
    return (
      <PageNoData
        title="No Playlists Found"
        message="No Spotify playlists were found for this username"
        icon={Heart}
        iconBgColor="bg-pink-500/20"
      />
    );
  }

  if (playlists.length === 0) {
    return (
      <PageNoData
        title="No Playlists Available"
        message="This Spotify user doesn't have any public playlists to import"
        icon={Heart}
        iconBgColor="bg-pink-500/20"
      />
    );
  }

  return (
    <SpotifyPlaylistsList
      playlists={playlists}
      spotifyUsername={spotifyUsername}
    />
  );
};
