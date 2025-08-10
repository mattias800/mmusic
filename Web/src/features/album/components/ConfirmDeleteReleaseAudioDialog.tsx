import * as React from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog.tsx";
import { Button } from "@/components/ui/button.tsx";

export interface ConfirmDeleteReleaseAudioDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  hasAnyAudio: boolean;
  onConfirmDelete: () => void;
}

export const ConfirmDeleteReleaseAudioDialog: React.FC<
  ConfirmDeleteReleaseAudioDialogProps
> = ({ open, onOpenChange, hasAnyAudio, onConfirmDelete }) => {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {hasAnyAudio
              ? "Delete all audio files?"
              : "No audio files to delete"}
          </DialogTitle>
          <DialogDescription>
            {hasAnyAudio
              ? "Are you sure you want to delete all audio files for this release? This will remove the files from disk and clear references in release.json."
              : "There are no audio files referenced in this release. No action will be taken."}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Close
          </Button>
          {hasAnyAudio && (
            <Button
              variant="destructive"
              onClick={() => {
                onConfirmDelete();
                onOpenChange(false);
              }}
            >
              Delete audio files
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
