import * as React from "react";
import { useState } from "react";
import { Button } from "./button"; // Assuming you have a Button component
import { Input } from "./input"; // Assuming you have an Input component
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "./dialog"; // Assuming you have a Dialog component

export interface RenamePromptProps {
  currentName: string;
  onRequestClose: () => void;
  onRename: (newName: string) => void;
  promptTitle?: string;
  inputLabel?: string;
  confirmText?: string;
  cancelText?: string;
}

export const RenamePrompt: React.FC<RenamePromptProps> = ({
  currentName,
  onRequestClose,
  onRename,
  promptTitle = "Rename Item",
  inputLabel = "New name",
  confirmText = "Rename",
  cancelText = "Cancel",
}) => {
  const [newName, setNewName] = useState(currentName);

  const handleRename = () => {
    if (newName.trim() && newName.trim() !== currentName) {
      onRename(newName.trim());
    }
    onRequestClose();
  };

  const handleKeyPress = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      handleRename();
    }
  };

  return (
    <Dialog open={true} onOpenChange={(open) => !open && onRequestClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{promptTitle}</DialogTitle>
        </DialogHeader>
        <div className="py-4">
          <label htmlFor="newNameInput" className="sr-only">
            {inputLabel}
          </label>
          <Input
            id="newNameInput"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            onKeyPress={handleKeyPress}
            placeholder={inputLabel}
            autoFocus
          />
        </div>
        <DialogFooter>
          <DialogClose asChild>
            <Button variant="outline" onClick={onRequestClose}>
              {cancelText}
            </Button>
          </DialogClose>
          <Button onClick={handleRename}>{confirmText}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
