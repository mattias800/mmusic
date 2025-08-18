import * as React from "react";
import { cn } from "@/lib/utils.ts";

export interface TabItemProps {
  onClick: () => void;
  isActive: boolean;
  label: string;
  icon?: React.ComponentType<{ className?: string }>;
}

export const TabItem: React.FC<TabItemProps> = ({
  onClick,
  icon: Icon,
  label,
  isActive,
}) => {
  return (
    <button
      onClick={onClick}
      className={cn(
        "flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all duration-200",
        "hover:bg-white/10 hover:text-white",
        isActive
          ? "bg-gradient-to-r from-blue-500/20 to-purple-500/20 text-white border border-blue-500/30 shadow-lg"
          : "text-gray-300 hover:text-white",
      )}
    >
      {Icon && <Icon className="w-4 h-4" />}
      {label}
    </button>
  );
};
