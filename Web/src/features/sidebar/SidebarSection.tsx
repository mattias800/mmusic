import * as React from "react";
import { PropsWithChildren } from "react";
import { SidebarSectionHeading } from "@/features/sidebar/SidebarSectionHeading.tsx";

export interface SidebarSectionProps extends PropsWithChildren {
  heading: string;
  right?: React.ReactNode;
}

export const SidebarSection: React.FC<SidebarSectionProps> = ({
  heading,
  children,
  right,
}) => {
  return (
    <div className="px-3 py-2">
      {heading && (
        <SidebarSectionHeading right={right}>{heading}</SidebarSectionHeading>
      )}
      <div className="flex flex-col gap-1">{children}</div>
    </div>
  );
};
