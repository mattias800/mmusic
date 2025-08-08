import * as React from "react";
import { PropsWithChildren } from "react";

export interface TagProps extends PropsWithChildren {}

export const Tag: React.FC<TagProps> = ({ children }) => {
  return (
    <span className="inline-flex items-center rounded bg-red-600/20 px-2 py-[2px] text-xs text-red-400">
      {children}
    </span>
  );
};
