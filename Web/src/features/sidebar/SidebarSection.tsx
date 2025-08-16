import * as React from "react";
import { PropsWithChildren } from "react";
import { SidebarSectionHeading } from "@/features/sidebar/SidebarSectionHeading.tsx";
import { LucideIcon } from "lucide-react";

export interface SidebarSectionProps extends PropsWithChildren {
  heading: string;
  right?: React.ReactNode;
  icon?: LucideIcon;
  iconColor?: string;
}

export const SidebarSection: React.FC<SidebarSectionProps> = ({
  heading,
  children,
  right,
  icon: Icon,
  iconColor = "text-gray-400",
}) => {
  return (
    <div className="space-y-3">
      {heading && (
        <SidebarSectionHeading right={right} icon={Icon} iconColor={iconColor}>
          {heading}
        </SidebarSectionHeading>
      )}
      <div className="flex flex-col gap-1">{children}</div>
    </div>
  );
};
