import type { ComponentType } from "react";
import * as React from "react";
import { Button } from "@/components/ui/button.tsx";

export interface SidebarActionButtonProps {
  onClick?: () => void;
  label: string;
  icon: ComponentType<{ className: string }>;
}

export const SidebarActionButton: React.FC<SidebarActionButtonProps> = ({
  label,
  onClick,
  icon,
}) => {
  const IconComponent = icon;
  return (
    <Button
      variant={"ghost"}
      className="w-full justify-start"
      onClick={onClick}
    >
      <IconComponent className="mr-2 h-4 w-4" />
      {label}
    </Button>
  );
};
