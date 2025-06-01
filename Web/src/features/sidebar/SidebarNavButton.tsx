import type { ComponentType } from "react";
import * as React from "react";
import { NavLink, useLocation } from "react-router";
import { Button } from "@/components/ui/button.tsx";
import { cn } from "@/lib/utils.ts";

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

  return (
    <Button
      variant={pathname === path ? "secondary" : "ghost"}
      className="w-full justify-start"
      iconLeft={icon}
      asChild
    >
      <NavLink
        to={path}
        role="button" // optional for a11y
        className={({ isActive }) =>
          cn(
            "flex items-center w-full",
            isActive ? "bg-secondary text-secondary-foreground" : "",
          )
        }
      >
        {label}
      </NavLink>
    </Button>
  );
};
