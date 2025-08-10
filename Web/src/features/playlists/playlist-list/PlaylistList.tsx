import { graphql } from "@/gql";
import * as React from "react";
import { useMutation, useQuery } from "urql";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { MessageBox } from "@/components/errors/MessageBox.tsx";
import { CreatePlaylistButton } from "@/features/playlists/CreatePlaylistButton.tsx";
import { PlaylistNavButton } from "@/features/playlists/playlist-list/PlaylistNavButton.tsx";

export interface PlaylistListProps {}

const playlistListQuery = graphql(`
  query PlaylistList {
    viewer {
      id
      playlists {
        id
        name
        createdAt
      }
    }
  }
`);

const renamePlaylistMutation = graphql(`
  mutation RenamePlaylist($playlistId: String!, $newPlaylistName: String!) {
    renamePlaylist(
      input: { playlistId: $playlistId, newPlaylistName: $newPlaylistName }
    ) {
      __typename
      ... on RenamePlaylistSuccess {
        playlist { id name createdAt }
      }
    }
  }
`);

const deletePlaylistMutation = graphql(`
  mutation DeletePlaylist($playlistId: String!) {
    deletePlaylist(input: { playlistId: $playlistId }) {
      __typename
      ... on DeletePlaylistSuccess {
        deletedPlaylistId
      }
    }
  }
`);

export const PlaylistList: React.FC<PlaylistListProps> = () => {
  const [{ data, fetching, error }] = useQuery({ query: playlistListQuery });
  const [, renamePlaylist] = useMutation(renamePlaylistMutation);
  const [, deletePlaylist] = useMutation(deletePlaylistMutation);

  if (fetching) {
    return <Spinner />;
  }

  if (error || !data) {
    return (
      <MessageBox variant={"error"} message={"Could not fetch playlists."} />
    );
  }

  const handleRenamePlaylist = async (
    playlistId: string,
    newPlaylistName: string,
  ) => {
    await renamePlaylist({ playlistId, newPlaylistName });
  };

  const handleDeletePlaylist = async (playlistId: string) => {
    await deletePlaylist({ playlistId });
  };

  return (
    <>
      <CreatePlaylistButton />

      {data.viewer?.playlists.map((playlist) => (
        <PlaylistNavButton
          key={playlist.id}
          playlistId={playlist.id}
          playlistName={playlist.name}
          onRenamePlaylist={handleRenamePlaylist}
          onDeletePlaylist={handleDeletePlaylist}
        />
      ))}
    </>
  );
};
