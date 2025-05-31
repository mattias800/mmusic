import * as React from "react";

export interface TxtProps {
  children: string | string[];
}

export const Txt: React.FC<TxtProps> = ({ children }) => {
  return <span className="text-white">{children}</span>;
};
