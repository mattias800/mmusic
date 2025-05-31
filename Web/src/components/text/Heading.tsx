import * as React from "react";

export interface HeadingProps {
  children: string | string[];
}

export const Heading: React.FC<HeadingProps> = ({ children }) => {
  return <h1 className={"text-white text-2xl font-bold"}>{children}</h1>;
};
