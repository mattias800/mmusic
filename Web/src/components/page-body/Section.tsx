import * as React from "react";
import { PropsWithChildren } from "react";

export interface SectionProps extends PropsWithChildren {}

export const Section: React.FC<SectionProps> = ({ children }) => {
  return <div className={"flex flex-col gap-4"}>{children}</div>;
};
