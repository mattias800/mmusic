import * as React from "react";
import { Button } from "@/components/ui/button.tsx";
import { useSidebar } from "@/components/ui/sidebar/SidebarContext.tsx";
import { cn } from "@/lib/utils.ts";
import { PanelLeftIcon } from "lucide-react";

function SidebarTrigger({
  className,
  onClick,
  ...props
}: React.ComponentProps<typeof Button>) {
  const { toggleSidebar } = useSidebar();

  return (
    <Button
      data-sidebar="trigger"
      data-slot="sidebar-trigger"
      variant="ghost"
      size="icon"
      className={cn("size-7", className)}
      onClick={(event) => {
        onClick?.(event);
        toggleSidebar();
      }}
      {...props}
    >
      <PanelLeftIcon />
      <span className="sr-only">Toggle Sidebar</span>
    </Button>
  );
}

export { SidebarTrigger };
