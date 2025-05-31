import type { ComponentType } from "react";
import * as React from "react";
import { Button } from "@/components/ui/button.tsx";
import { LoaderCircle } from "lucide-react";

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
  const IconComponent = icon;
  return (
    <Button
      variant={"ghost"}
      className="w-full justify-start"
      onClick={onClick}
    >
      {loading ? (
        <LoaderCircle className="animate-spin mr-2 h-4 w-4" />
      ) : (
        <IconComponent className="mr-2 h-4 w-4" />
      )}
      {label}
    </Button>
  );
};
