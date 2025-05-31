import * as React from "react";
import { ListMusic } from "lucide-react";
import { SidebarNavButton } from "@/features/sidebar/SidebarNavButton.tsx";
import {
  ContextMenu,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuTrigger,
} from "@/components/ui/context-menu.tsx";

export interface PlaylistNavButtonProps {
  playlistName: string | undefined | null;
  playlistId: string;
  onClickRenamePlaylist: () => void;
  onClickDeletePlaylist: () => void;
}

export const PlaylistNavButton: React.FC<PlaylistNavButtonProps> = ({
  playlistId,
  playlistName,
  onClickDeletePlaylist,
  onClickRenamePlaylist,
}) => {
  return (
    <ContextMenu>
      <ContextMenuTrigger>
        <SidebarNavButton
          path={"/playlist/" + playlistId}
          label={playlistName ?? "New playlist"}
          icon={ListMusic}
        />
      </ContextMenuTrigger>
      <ContextMenuContent className="w-40">
        <ContextMenuItem onClick={onClickRenamePlaylist}>
          Rename playlist
        </ContextMenuItem>
        <ContextMenuItem onClick={onClickDeletePlaylist}>
          Delete playlist
        </ContextMenuItem>
      </ContextMenuContent>
    </ContextMenu>
  );
};
