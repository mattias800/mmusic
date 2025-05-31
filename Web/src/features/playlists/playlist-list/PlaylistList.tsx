import { graphql } from "@/gql";
import * as React from "react";
import { useQuery, useMutation } from "urql";
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

const renamePlaylistMutationDoc = graphql(`
mutation RenamePlaylist($playlistId: String!, $newPlaylistName: String!) {
  renamePlaylist(input: {playlistId: $playlistId, newPlaylistName: $newPlaylistName}) {
    __typename
    ... on RenamePlaylistSuccess {
      viewer {
        id
        playlists {
          id
          name
          createdAt
        }
      }
    }
  }
}`);


export const PlaylistList: React.FC<PlaylistListProps> = () => {
  const [{ data, fetching, error }] = useQuery({ query: playlistListQuery });
  const [, renamePlaylist] = useMutation(renamePlaylistMutationDoc);

  if (fetching) {
    return <Spinner />;
  }

  if (error || !data) {
    return (
      <MessageBox variant={"error"} message={"Could not fetch playlists."} />
    );
  }

  const handleRenamePlaylist = async (playlistId: string, newPlaylistName: string) => {
    await renamePlaylist({ playlistId, newPlaylistName });
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
          onClickDeletePlaylist={() => {
            console.log("Delete playlist", playlist.id);
          }}
        />
      ))}
    </>
  );
};
