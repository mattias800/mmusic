import { graphql } from "@/gql";
import * as React from "react";
import { useQuery } from "urql";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { MessageBox } from "@/components/errors/MessageBox.tsx";
import { CreatePlaylistButton } from "@/features/playlists/CreatePlaylistButton.tsx";
import { SidebarNavButton } from "@/features/sidebar/SidebarNavButton.tsx";
import { ListMusic } from "lucide-react";

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

export const PlaylistList: React.FC<PlaylistListProps> = () => {
  const [{ data, fetching, error }] = useQuery({ query: playlistListQuery });

  if (fetching) {
    return <Spinner />;
  }

  if (error || !data) {
    return (
      <MessageBox variant={"error"} message={"Could not fetch playlists."} />
    );
  }

  return (
    <>
      <CreatePlaylistButton />

      {data.viewer?.playlists.map((playlist) => (
        <SidebarNavButton
          path={"/playlist/" + playlist.id}
          label={playlist.name ?? "New playlist"}
          icon={ListMusic}
        />
      ))}
    </>
  );
};
