import * as React from "react";
import { Button } from "./button";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "./dialog";

export interface ConfirmDeletePromptProps {
  onClose: () => void;
  itemName: string;
  onConfirmDelete: () => void;
  promptTitle?: string;
  confirmText?: string;
  cancelText?: string;
}

export const ConfirmDeletePrompt: React.FC<ConfirmDeletePromptProps> = ({
  itemName,
  onClose,
  onConfirmDelete,
  promptTitle = "Confirm Deletion",
  confirmText = "Delete",
  cancelText = "Cancel",
}) => {
  return (
    <Dialog open={true} onOpenChange={(open) => !open && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{promptTitle}</DialogTitle>
          <DialogDescription>
            Are you sure you want to delete "{itemName}"? This action cannot be
            undone.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter className="mt-4">
          {" "}
          {/* Added margin for spacing */}
          <DialogClose asChild>
            <Button variant="outline" onClick={onClose}>
              {cancelText}
            </Button>
          </DialogClose>
          {/* Assuming your Button component accepts a variant like "destructive" */}
          <Button
            variant="destructive"
            onClick={() => {
              onConfirmDelete();
              onClose();
            }}
          >
            {confirmText}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
