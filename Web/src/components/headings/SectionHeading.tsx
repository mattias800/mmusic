import * as React from "react";

export interface SectionHeadingProps {
  children: string | string[];
}

export const SectionHeading: React.FC<SectionHeadingProps> = ({ children }) => {
  return <h2 className="text-xl font-semibold mb-4">{children}</h2>;
};
