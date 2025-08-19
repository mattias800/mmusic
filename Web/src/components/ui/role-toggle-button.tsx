import * as React from "react";
import { cn } from "@/lib/utils";
import { Shield, Check, Minus } from "lucide-react";

export interface RoleToggleButtonProps {
  label: string;
  active: boolean;
  onToggle: () => void;
  disabled?: boolean;
  icon?: React.ComponentType<{ className: string }>;
}

export const RoleToggleButton: React.FC<RoleToggleButtonProps> = ({
  label,
  active,
  onToggle,
  disabled,
  icon: Icon = Shield,
}) => {
  return (
    <button
      type="button"
      disabled={disabled}
      onClick={onToggle}
      className={cn(
        "inline-flex items-center gap-2 px-3 py-2 rounded-lg border transition-all",
        "bg-white/5 text-gray-200 border-white/10 hover:bg-white/10 hover:border-white/20",
        active &&
          "bg-gradient-to-r from-blue-500/20 to-purple-500/20 text-white border-blue-500/30",
        disabled && "opacity-60 cursor-not-allowed",
      )}
      aria-pressed={active}
    >
      <span
        className={cn(
          "p-1 rounded-md",
          active ? "bg-blue-500/20" : "bg-white/10",
        )}
      >
        {active ? (
          <Check className="w-4 h-4 text-blue-300" />
        ) : (
          <Minus className="w-4 h-4 text-gray-400" />
        )}
      </span>
      <Icon
        className={cn("w-4 h-4", active ? "text-blue-300" : "text-gray-400")}
      />
      <span className="text-sm font-medium">{label}</span>
    </button>
  );
};
