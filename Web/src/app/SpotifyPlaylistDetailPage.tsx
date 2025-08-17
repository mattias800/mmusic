import React from "react";
import { useParams } from "react-router";
import { SpotifyPlaylistPanel } from "../features/spotify-import/playlist-detail/SpotifyPlaylistPanel.tsx";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { PageLoading, PageError, PageNoData } from "@/components/ui";
import { Music, AlertTriangle, Heart } from "lucide-react";

const spotifyPlaylistDetailsQuery = graphql(`
  query SpotifyPlaylistDetails($playlistId: String!) {
    playlist {
      importPlaylists {
        spotify {
          byId: spotifyPlaylistById(id: $playlistId) {
            id
            ...SpotifyPlaylistPanel_SpotifyPlaylist
          }
        }
      }
    }
  }
`);

export const SpotifyPlaylistDetailPage: React.FC = () => {
  const { playlistId } = useParams<{ playlistId: string }>();

  const [{ data, fetching, error }] = useQuery({
    query: spotifyPlaylistDetailsQuery,
    variables: { playlistId: playlistId! },
    requestPolicy: "network-only",
    pause: !playlistId,
  });

  if (!playlistId) {
    return <PageError 
      title="Invalid Playlist ID" 
      message="The Spotify playlist ID provided is not valid"
      icon={AlertTriangle}
      iconBgColor="bg-red-500/20"
    />;
  }

  if (fetching) {
    return <PageLoading 
      title="Loading Spotify Playlist" 
      subtitle="Fetching playlist details and tracks"
      icon={Music}
      iconBgColor="bg-green-500/20"
    />;
  }

  if (error) {
    return <PageError 
      title="Failed to Load Playlist" 
      message="We couldn't load the Spotify playlist details"
      error={error}
      icon={AlertTriangle}
      iconBgColor="bg-red-500/20"
    />;
  }

  if (!data?.playlist.importPlaylists.spotify.byId) {
    return <PageNoData 
      title="Playlist Not Found" 
      message="The Spotify playlist you're looking for doesn't exist or may have been removed"
      icon={Heart}
      iconBgColor="bg-yellow-500/20"
    />;
  }

  return (
    <SpotifyPlaylistPanel
      playlist={data.playlist.importPlaylists.spotify.byId}
    />
  );
};
