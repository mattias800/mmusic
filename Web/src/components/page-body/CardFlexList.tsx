import * as React from "react";
import { PropsWithChildren } from "react";

export interface CardFlexListProps extends PropsWithChildren {}

export const CardFlexList: React.FC<CardFlexListProps> = ({ children }) => {
  return <div className={"flex gap-8 flex-wrap"}>{children}</div>;
};
