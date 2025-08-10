import React from "react";
import { useParams } from "react-router";
import { SpotifyPlaylistPanel } from "../features/spotify-import/playlist-detail/SpotifyPlaylistPanel.tsx";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";

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

  if (fetching)
    return (
      <SpinnerSpacing>
        <Spinner />
      </SpinnerSpacing>
    );

  if (error) {
    return <div className="p-4 text-red-400">Error: {error.message}</div>;
  }

  if (!playlistId) {
    return <div className="p-4">Invalid playlist id.</div>;
  }

  if (!data?.playlist.importPlaylists.spotify.byId) {
    return <div className="p-4">Could not find playlist.</div>;
  }

  return (
    <SpotifyPlaylistPanel
      playlist={data.playlist.importPlaylists.spotify.byId}
    />
  );
};
