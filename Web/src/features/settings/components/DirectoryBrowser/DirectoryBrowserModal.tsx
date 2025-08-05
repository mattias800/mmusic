import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useState } from "react";
import { DirectoryTreeView } from "./DirectoryTreeView";
import { Input } from "@/components/ui/input";

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSelect: (path: string) => void;
  heading: string;
}

export function DirectoryBrowserModal({
  open,
  onOpenChange,
  onSelect,
  heading,
}: Props) {
  const [selectedPath, setSelectedPath] = useState<string | undefined>(
    undefined,
  );
  const [newFolderName, setNewFolderName] = useState("");
  const [showNewFolderInput, setShowNewFolderInput] = useState(false);
  // const [, createDirectory] = useMutation(CreateDirectoryDocument);

  const handleSelect = () => {
    if (selectedPath) {
      onSelect(selectedPath);
      onOpenChange(false);
    }
  };

  const handleCreateFolder = async () => {
    if (newFolderName && selectedPath) {
      // const fullPath = `${selectedPath}/${newFolderName}`;
      // await createDirectory({ path: fullPath });
      setNewFolderName("");
      setShowNewFolderInput(false);
      // Optionally, refresh the current directory view
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[625px]">
        <DialogHeader>
          <DialogTitle>{heading}</DialogTitle>
        </DialogHeader>
        <div className="max-h-[60vh] overflow-y-auto">
          <DirectoryTreeView
            onSelect={setSelectedPath}
            selectedPath={selectedPath}
          />
        </div>
        <DialogFooter>
          {showNewFolderInput ? (
            <div className="flex w-full space-x-2">
              <Input
                placeholder="New folder name"
                value={newFolderName}
                onChange={(e) => setNewFolderName(e.target.value)}
              />
              <Button onClick={handleCreateFolder}>Create</Button>
              <Button
                variant="outline"
                onClick={() => setShowNewFolderInput(false)}
              >
                Cancel
              </Button>
            </div>
          ) : (
            <div className="flex w-full justify-between">
              <Button
                variant="outline"
                onClick={() => setShowNewFolderInput(true)}
              >
                New Folder
              </Button>
              <div className="space-x-2">
                <Button variant="outline" onClick={() => onOpenChange(false)}>
                  Cancel
                </Button>
                <Button onClick={handleSelect} disabled={!selectedPath}>
                  Select
                </Button>
              </div>
            </div>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
