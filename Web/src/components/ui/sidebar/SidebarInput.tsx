import * as React from "react";
import { Input } from "@/components/ui/input.tsx";
import { cn } from "@/lib/utils.ts";

function SidebarInput({
  className,
  ...props
}: React.ComponentProps<typeof Input>) {
  return (
    <Input
      data-slot="sidebar-input"
      data-sidebar="input"
      className={cn("bg-background h-8 w-full shadow-none", className)}
      {...props}
    />
  );
}

export { SidebarInput };
