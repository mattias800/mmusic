import type { ComponentType } from "react";
import * as React from "react";
import { NavLink, useLocation } from "react-router";
import { cn } from "@/lib/utils.ts";

export interface SidebarNavButtonProps {
  path: string;
  label: string;
  icon: ComponentType<{ className: string }>;
}

export const SidebarNavButton: React.FC<SidebarNavButtonProps> = ({
  label,
  path,
  icon: Icon,
}) => {
  const { pathname } = useLocation();
  const isActive = pathname === path;

  return (
    <NavLink
      to={path}
      role="button"
      className={cn(
        "flex items-center w-full px-4 py-3 rounded-lg transition-all duration-200 group",
        "hover:bg-white/10 hover:border-white/20",
        isActive
          ? "bg-gradient-to-r from-blue-500/20 to-purple-500/20 border border-blue-500/30 text-white"
          : "text-gray-300 hover:text-white border border-transparent",
      )}
    >
      <Icon
        className={cn(
          "w-5 h-5 mr-4 transition-colors duration-200",
          isActive
            ? "text-blue-400"
            : "text-gray-400 group-hover:text-gray-300",
        )}
      />
      <span className="text-base font-medium">{label}</span>
    </NavLink>
  );
};
