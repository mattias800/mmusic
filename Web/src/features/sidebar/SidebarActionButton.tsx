import type { ComponentType } from "react";
import * as React from "react";
import { Button } from "@/components/ui/button.tsx";

export interface SidebarActionButtonProps {
  onClick?: () => void;
  label: string;
  icon: ComponentType<{ className: string }>;
  loading?: boolean;
}

export const SidebarActionButton: React.FC<SidebarActionButtonProps> = ({
  label,
  onClick,
  icon,
  loading,
}) => {
  return (
    <Button
      variant={"ghost"}
      className="w-full justify-start"
      loading={loading}
      iconLeft={icon}
      onClick={onClick}
    >
      {label}
    </Button>
  );
};
