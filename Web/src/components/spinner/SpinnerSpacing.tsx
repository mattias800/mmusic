import * as React from "react";
import { PropsWithChildren } from "react";

export const SpinnerSpacing: React.FC<PropsWithChildren> = ({ children }) => {
  return (
    <div className={"flex justify-center items-center py-16"}>{children}</div>
  );
};
