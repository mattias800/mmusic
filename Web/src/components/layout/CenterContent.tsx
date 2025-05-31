import * as React from "react";
import { PropsWithChildren } from "react";

export interface CenterContentProps extends PropsWithChildren {}

export const CenterContent: React.FC<CenterContentProps> = ({ children }) => {
  return (
    <div className="flex min-h-full flex-col justify-center items-center px-6 py-12 lg:px-8  text-white">
      {children}
    </div>
  );
};
