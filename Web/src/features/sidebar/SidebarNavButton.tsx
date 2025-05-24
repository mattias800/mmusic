import type { ComponentType } from "react";
import * as React from "react";
import { NavLink, useLocation } from "react-router";
import { Button } from "@/components/ui/button.tsx";

export interface SidebarNavButtonProps {
  path: string;
  label: string;
  icon: ComponentType<{ className: string }>;
}

export const SidebarNavButton: React.FC<SidebarNavButtonProps> = ({
  label,
  path,
  icon,
}) => {
  const { pathname } = useLocation();

  const IconComponent = icon;
  return (
    <Button
      variant={pathname === path ? "secondary" : "ghost"}
      className="w-full justify-start"
      asChild
    >
      <NavLink to={path}>
        <IconComponent className="mr-2 h-4 w-4" />
        {label}
      </NavLink>
    </Button>
  );
};
