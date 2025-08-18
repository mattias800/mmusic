import * as React from "react";
import { PropsWithChildren } from "react";
import { cn } from "@/lib/utils";

export interface TabMenuProps extends PropsWithChildren {
  className?: string;
}

export const TabMenu: React.FC<TabMenuProps> = ({ className, children }) => {
  return (
    <div className={cn("space-y-6", className)}>
      {/* Tab Navigation */}
      <div className="flex space-x-1 bg-white/5 backdrop-blur-sm rounded-xl p-1 border border-white/10">
        {children}
      </div>
    </div>
  );
};
