import * as React from "react";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { PhotoCardBottomText } from "@/components/cards/PhotoCardBottomText.tsx";
import { useNavigate } from "react-router";
import { FragmentType, graphql, useFragment } from "@/gql";

export interface PlaylistCardProps {
  playlist: FragmentType<typeof playlistCardPlaylistFragment>;
}

const playlistCardPlaylistFragment = graphql(`
  fragment PlaylistCard_Playlist on Playlist {
    id
    name
    coverImageUrl
    createdAt
  }
`);

export const PlaylistCard: React.FC<PlaylistCardProps> = (props) => {
  const playlist = useFragment(playlistCardPlaylistFragment, props.playlist);

  const navigate = useNavigate();
  return (
    <PhotoCard
      imageUrl={playlist.coverImageUrl ?? ""}
      imageAlt={playlist.name ?? "Playlist cover"}
      onClick={() => navigate(`/playlist/${playlist.id}`)}
    >
      <PhotoCardCenterHeading>
        {playlist.name ?? "Untitled Playlist"}
      </PhotoCardCenterHeading>
      {playlist.createdAt && (
        <PhotoCardBottomText>
          {new Date(playlist.createdAt).toLocaleDateString()}
        </PhotoCardBottomText>
      )}
    </PhotoCard>
  );
};
