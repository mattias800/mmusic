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
import { useMutation } from "urql";

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
  const [isDragOver, setIsDragOver] = useState(false);

  const [, addTrackToPlaylist] = useMutation(`
    mutation AddTrackToPlaylist(
      $playlistId: UUID!
      $artistId: String!
      $releaseFolderName: String!
      $trackNumber: Int!
    ) {
      addTrackToPlaylist(
        playlistId: $playlistId
        artistId: $artistId
        releaseFolderName: $releaseFolderName
        trackNumber: $trackNumber
      ) {
        __typename
        ... on AddTrackToPlaylistSuccess { success }
        ... on AddTrackToPlaylistError { message }
      }
    }
  `);

  const handleRename = (newName: string) => {
    onRenamePlaylist(playlistId, newName);
    setIsRenamePromptOpen(false);
  };

  const handleDeleteConfirm = () => {
    onDeletePlaylist(playlistId);
    setIsDeletePromptOpen(false);
  };

  const onDrop = React.useCallback(
    async (ev: React.DragEvent<HTMLDivElement>) => {
      ev.preventDefault();
      setIsDragOver(false);
      const data = ev.dataTransfer.getData("application/json");
      if (!data) return;
      try {
        const parsed = JSON.parse(data) as {
          type?: string;
          artistId?: string;
          releaseFolderName?: string;
          trackNumber?: number;
        };
        if (parsed?.type !== "track") return;
        if (
          !parsed.artistId ||
          !parsed.releaseFolderName ||
          typeof parsed.trackNumber !== "number"
        ) {
          console.warn("Drop payload missing required fields", parsed);
          return;
        }
        const result = await addTrackToPlaylist({
          playlistId,
          artistId: parsed.artistId,
          releaseFolderName: parsed.releaseFolderName,
          trackNumber: parsed.trackNumber,
        });
        if (result.error) {
          console.error("Failed to add track to playlist:", result.error);
        } else if (
          result.data?.addTrackToPlaylist?.__typename ===
          "AddTrackToPlaylistError"
        ) {
          const msg = (
            result.data.addTrackToPlaylist as {
              __typename: string;
              message?: string;
            }
          ).message;
          console.error("AddTrackToPlaylist error:", msg ?? "unknown error");
        } else {
          console.log("Track added to playlist", {
            playlistId,
            artistId: parsed.artistId,
            releaseFolderName: parsed.releaseFolderName,
            trackNumber: parsed.trackNumber,
          });
        }
      } catch (e) {
        console.error("Invalid drag payload for playlist drop", e);
      }
    },
    [addTrackToPlaylist, playlistId],
  );

  return (
    <>
      <ContextMenu>
        <ContextMenuTrigger>
          <div
            onDragEnter={() => setIsDragOver(true)}
            onDragOver={(e) => {
              e.preventDefault();
              if (!isDragOver) setIsDragOver(true);
            }}
            onDragLeave={() => setIsDragOver(false)}
            onDrop={onDrop}
            className={
              isDragOver
                ? "rounded-md ring-2 ring-green-500/70 bg-green-500/10"
                : ""
            }
          >
            <SidebarNavButton
              path={"/playlist/" + playlistId}
              label={playlistName ?? "New playlist"}
              icon={ListMusic}
            />
          </div>
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
