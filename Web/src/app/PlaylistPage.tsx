import * as React from "react";
import { useParams } from "react-router";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { PlaylistPanel } from "@/features/playlists/PlaylistPanel.tsx";

const playlistQuery = graphql(`
  query PlaylistQuery($playlistId: ID!) {
    playlist {
      byId(playlistId: $playlistId) {
        id
        name
        ...PlaylistPanel_Playlist
      }
    }
  }
`);

export const PlaylistPage: React.FC = () => {
  const { playlistId } = useParams<{ playlistId: string }>();

  const [{ data, fetching, error, stale }] = useQuery({
    query: playlistQuery,
    variables: { playlistId: playlistId! },
    pause: !playlistId,
  });

  if (!playlistId) return "Invalid playlist ID";
  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;

  if (!data?.playlist.byId) {
    return <div>Playlist not found.</div>;
  }

  return (
    <>
      <title>{data.playlist.byId.name ?? "Playlist"}</title>
      <PlaylistPanel playlist={data.playlist.byId} />
    </>
  );
};
