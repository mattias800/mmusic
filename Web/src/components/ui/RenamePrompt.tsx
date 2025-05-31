import * as React from "react";
import { useState } from "react";
import { Button } from "./button"; // Assuming you have a Button component
import { Input } from "./input"; // Assuming you have an Input component
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogClose,
} from "./dialog"; // Assuming you have a Dialog component

export interface RenamePromptProps {
  isOpen: boolean;
  currentName: string;
  onClose: () => void;
  onRename: (newName: string) => void;
  promptTitle?: string;
  inputLabel?: string;
  confirmText?: string;
  cancelText?: string;
}

export const RenamePrompt: React.FC<RenamePromptProps> = ({
  isOpen,
  currentName,
  onClose,
  onRename,
  promptTitle = "Rename Item",
  inputLabel = "New name",
  confirmText = "Rename",
  cancelText = "Cancel",
}) => {
  const [newName, setNewName] = useState(currentName);

  React.useEffect(() => {
    if (isOpen) {
      setNewName(currentName);
    }
  }, [isOpen, currentName]);

  const handleRename = () => {
    if (newName.trim() && newName.trim() !== currentName) {
      onRename(newName.trim());
    }
    onClose();
  };

  const handleKeyPress = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      handleRename();
    }
  };

  if (!isOpen) {
    return null;
  }

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
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
            <Button variant="outline" onClick={onClose}>
              {cancelText}
            </Button>
          </DialogClose>
          <Button onClick={handleRename}>{confirmText}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}; 