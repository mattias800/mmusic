import { graphql } from "@/gql";
import * as React from "react";
import { PlusCircleIcon } from "lucide-react";
import { useMutation } from "urql";
import { SidebarActionButton } from "@/features/sidebar/SidebarActionButton.tsx";

export interface CreatePlaylistButtonProps {}

const mutation = graphql(`
  mutation CreatePlaylist {
    createPlaylist {
      __typename
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
    }
  }
`);

export const CreatePlaylistButton: React.FC<CreatePlaylistButtonProps> = () => {
  const [{ fetching }, createPlaylist] = useMutation(mutation);
  return (
    <SidebarActionButton
      label={"Create new playlist"}
      icon={PlusCircleIcon}
      loading={fetching}
      onClick={() => createPlaylist({})}
    />
  );
};
