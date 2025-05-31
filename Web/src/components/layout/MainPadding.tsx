import * as React from "react";
import { PropsWithChildren } from "react";

export interface MainPaddingProps extends PropsWithChildren {}

export const MainPadding: React.FC<MainPaddingProps> = ({ children }) => {
  return <div className={"px-6 py-6"}>{children}</div>;
};
