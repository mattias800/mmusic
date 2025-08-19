import * as React from "react";
import { LucideIcon } from "lucide-react";

export interface SidebarSectionHeadingProps {
  children: string;
  right?: React.ReactNode;
  icon?: LucideIcon;
  iconColor?: string;
}

export const SidebarSectionHeading: React.FC<SidebarSectionHeadingProps> = ({
  children,
  right,
  icon: Icon,
  iconColor = "text-gray-400",
}) => {
  return (
    <div className="flex items-center justify-between gap-2">
      <div className="flex items-center gap-3">
        {Icon && (
          <div className={`p-2 rounded-lg bg-white/5 border border-white/10`}>
            <Icon className={`w-5 h-5 ${iconColor}`} />
          </div>
        )}
        <h3 className="text-base font-semibold text-white tracking-tight">
          {children}
        </h3>
      </div>
      {right && <div className="flex items-center">{right}</div>}
    </div>
  );
};
