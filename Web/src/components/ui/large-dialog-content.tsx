import * as React from "react";
import { DialogContent } from "@/components/ui/dialog.tsx";
import { cn } from "@/lib/utils";

export const LargeDialogContent: React.FC<
  React.ComponentProps<typeof DialogContent>
> = ({ className, ...props }) => {
  return (
    <DialogContent
      className={cn("max-w-3xl overflow-hidden p-0", className)}
      {...props}
    />
  );
};


