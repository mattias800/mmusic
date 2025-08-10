import * as React from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog.tsx";
import { Button } from "@/components/ui/button.tsx";

interface FixMatchDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  artistId: string;
  releaseFolderName: string;
}

export const FixMatchDialog: React.FC<FixMatchDialogProps> = (props) => {
  return (
    <Dialog open={props.open} onOpenChange={props.onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Fix match</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <p className="text-sm text-white/70">
            This feature is not available in the current server schema.
          </p>
          <div className="flex justify-end">
            <Button size="sm" onClick={() => props.onOpenChange(false)}>
              Close
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

