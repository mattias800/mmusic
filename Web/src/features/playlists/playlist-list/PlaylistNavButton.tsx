import * as React from "react";
import { ListMusic } from "lucide-react";
import { SidebarNavButton } from "@/features/sidebar/SidebarNavButton.tsx";
import {
  ContextMenu,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuTrigger,
} from "@/components/ui/context-menu.tsx";
import { RenamePrompt } from "@/components/ui/RenamePrompt.tsx";
import { useState } from "react";

export interface PlaylistNavButtonProps {
  playlistName: string | undefined | null;
  playlistId: string;
  onRenamePlaylist: (playlistId: string, newName: string) => void;
  onClickDeletePlaylist: () => void;
}

export const PlaylistNavButton: React.FC<PlaylistNavButtonProps> = ({
  playlistId,
  playlistName,
  onClickDeletePlaylist,
  onRenamePlaylist,
}) => {
  const [isRenamePromptOpen, setIsRenamePromptOpen] = useState(false);

  const handleRename = (newName: string) => {
    onRenamePlaylist(playlistId, newName);
    setIsRenamePromptOpen(false);
  };

  return (
    <>
      <ContextMenu>
        <ContextMenuTrigger>
          <SidebarNavButton
            path={"/playlist/" + playlistId}
            label={playlistName ?? "New playlist"}
            icon={ListMusic}
          />
        </ContextMenuTrigger>
        <ContextMenuContent className="w-40">
          <ContextMenuItem onClick={() => setIsRenamePromptOpen(true)}>
            Rename playlist
          </ContextMenuItem>
          <ContextMenuItem onClick={onClickDeletePlaylist}>
            Delete playlist
          </ContextMenuItem>
        </ContextMenuContent>
      </ContextMenu>
      <RenamePrompt
        isOpen={isRenamePromptOpen}
        currentName={playlistName ?? ""}
        onClose={() => setIsRenamePromptOpen(false)}
        onRename={handleRename}
        promptTitle="Rename Playlist"
        inputLabel="New playlist name"
      />
    </>
  );
};
