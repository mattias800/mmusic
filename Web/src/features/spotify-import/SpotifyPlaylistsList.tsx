import React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";
import { SpotifyPlaylistCard } from "@/features/spotify-import/SpotifyPlaylistCard.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";

interface SpotifyPlaylistsListProps {
  playlists: Array<
    FragmentType<typeof userPlaylistsListSpotifyPlaylistFragment>
  >;
  spotifyUsername: string;
}

const importSpotifyPlaylistByIdMutation = graphql(`
  mutation ImportSpotifyPlaylistById($playlistId: String!, $userId: UUID!) {
    importSpotifyPlaylistById(playlistId: $playlistId, userId: $userId) {
      __typename
      ... on ImportSpotifyPlaylistSuccess {
        success
      }
      ... on ImportSpotifyPlaylistError {
        message
      }
    }
  }
`);

export const userPlaylistsListSpotifyPlaylistFragment = graphql(`
  fragment SpotifyPlaylistsList_SpotifyPlaylist on SpotifyPlaylist {
    id
    description
    name
    coverImageUrl
  }
`);

export const SpotifyPlaylistsList: React.FC<SpotifyPlaylistsListProps> = ({
  ...props
}) => {
  const playlists = useFragment(
    userPlaylistsListSpotifyPlaylistFragment,
    props.playlists,
  );

  const [{ fetching }] = useMutation(importSpotifyPlaylistByIdMutation);

  return (
    <CardFlexList>
      {playlists.map((playlist) => (
        <SpotifyPlaylistCard
          title={playlist.name}
          imageUrl={playlist.coverImageUrl ?? ""}
        />
      ))}
    </CardFlexList>
  );
};
