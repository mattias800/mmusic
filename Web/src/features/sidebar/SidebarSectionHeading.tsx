import * as React from "react";

export interface SidebarSectionHeadingProps {
  children: string;
  right?: React.ReactNode;
}

export const SidebarSectionHeading: React.FC<SidebarSectionHeadingProps> = ({
  children,
  right,
}) => {
  return (
    <h2 className="mb-2 px-4 text-lg font-semibold tracking-tight flex gap-4 justify-between">
      {children}
      {right}
    </h2>
  );
};
