import * as React from "react";
import { PropsWithChildren } from "react";

export interface SectionProps extends PropsWithChildren {}

export const Section: React.FC<SectionProps> = ({ children }) => {
  return <div className="px-6 md:px-10 mt-4">{children}</div>;
};
