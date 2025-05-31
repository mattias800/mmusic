import { graphql } from "@/gql";
import * as React from "react";
import { PlusCircleIcon } from "lucide-react";
import { useMutation } from "urql";
import { SidebarActionButton } from "@/features/sidebar/SidebarActionButton.tsx";

export interface CreatePlaylistButtonProps {}

const mutation = graphql(`
  mutation CreatePlaylist {
    createPlaylist {
      ... on CreatePlaylistSuccess {
        viewer {
          id
          playlists {
            id
            name
            createdAt
          }
        }
      }
      ... on CreatePlaylistNotAuthenticated {
        message
      }
    }
  }
`);

export const CreatePlaylistButton: React.FC<CreatePlaylistButtonProps> = () => {
  const [, createPlaylist] = useMutation(mutation);
  return (
    <SidebarActionButton
      label={"Create new playlist"}
      icon={PlusCircleIcon}
      onClick={() => createPlaylist({})}
    />
  );
};
