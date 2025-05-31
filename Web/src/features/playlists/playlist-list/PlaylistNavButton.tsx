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
}

export const PlaylistNavButton: React.FC<PlaylistNavButtonProps> = ({
  playlistId,
  playlistName,
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
        <ContextMenuItem>Rename playlist</ContextMenuItem>
        <ContextMenuItem>Delete playlist</ContextMenuItem>
      </ContextMenuContent>
    </ContextMenu>
  );
};
