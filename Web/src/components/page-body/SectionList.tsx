import * as React from "react";
import { PropsWithChildren } from "react";

export interface SectionListProps extends PropsWithChildren {}

export const SectionList: React.FC<SectionListProps> = ({ children }) => {
  return <div className={"flex flex-col gap-8"}>{children}</div>;
};
