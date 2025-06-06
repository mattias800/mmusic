import * as React from "react";
import { useState } from "react";
import { ListMusic } from "lucide-react";
import { SidebarNavButton } from "@/features/sidebar/SidebarNavButton.tsx";
import {
  ContextMenu,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuTrigger,
} from "@/components/ui/context-menu.tsx";
import { RenamePrompt } from "@/components/ui/RenamePrompt.tsx";
import { ConfirmDeletePrompt } from "@/components/ui/ConfirmDeletePrompt.tsx";

export interface PlaylistNavButtonProps {
  playlistName: string | undefined | null;
  playlistId: string;
  onRenamePlaylist: (playlistId: string, newName: string) => void;
  onDeletePlaylist: (playlistId: string) => void;
}

export const PlaylistNavButton: React.FC<PlaylistNavButtonProps> = ({
  playlistId,
  playlistName,
  onDeletePlaylist,
  onRenamePlaylist,
}) => {
  const [isRenamePromptOpen, setIsRenamePromptOpen] = useState(false);
  const [isDeletePromptOpen, setIsDeletePromptOpen] = useState(false);

  const handleRename = (newName: string) => {
    onRenamePlaylist(playlistId, newName);
    setIsRenamePromptOpen(false);
  };

  const handleDeleteConfirm = () => {
    onDeletePlaylist(playlistId);
    setIsDeletePromptOpen(false);
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
          <ContextMenuItem onClick={() => setIsDeletePromptOpen(true)}>
            Delete playlist
          </ContextMenuItem>
        </ContextMenuContent>
      </ContextMenu>
      {isRenamePromptOpen && (
        <RenamePrompt
          currentName={playlistName ?? ""}
          onRequestClose={() => setIsRenamePromptOpen(false)}
          onRename={handleRename}
          promptTitle="Rename Playlist"
          inputLabel="New playlist name"
        />
      )}
      {isDeletePromptOpen && (
        <ConfirmDeletePrompt
          itemName={playlistName ?? "this playlist"}
          onClose={() => setIsDeletePromptOpen(false)}
          onConfirmDelete={handleDeleteConfirm}
          promptTitle="Delete Playlist"
        />
      )}
    </>
  );
};
