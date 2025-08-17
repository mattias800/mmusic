import * as React from "react";
import { useParams } from "react-router";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { PageLoading, PageError, PageNoData } from "@/components/ui";
import { PlaylistPanel } from "@/features/playlists/PlaylistPanel.tsx";
import { Music, AlertTriangle, Heart } from "lucide-react";

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

  if (!playlistId) return <PageError 
    title="Invalid Playlist ID" 
    message="The playlist ID provided is not valid"
    icon={AlertTriangle}
    iconBgColor="bg-red-500/20"
  />;
  
  if (fetching || stale) return <PageLoading 
    title="Loading Playlist" 
    subtitle="Fetching playlist information and tracks"
    icon={Music}
    iconBgColor="bg-pink-500/20"
  />;
  
  if (error) return <PageError 
    title="Failed to Load Playlist" 
    message="We couldn't load the playlist information"
    error={error}
    icon={AlertTriangle}
    iconBgColor="bg-red-500/20"
  />;

  if (!data?.playlist.byId) {
    return <PageNoData 
      title="Playlist Not Found" 
      message="The playlist you're looking for doesn't exist or may have been removed"
      icon={Heart}
      iconBgColor="bg-yellow-500/20"
    />;
  }

  return (
    <>
      <title>{data.playlist.byId.name ?? "Playlist"}</title>
      <PlaylistPanel playlist={data.playlist.byId} />
    </>
  );
};
