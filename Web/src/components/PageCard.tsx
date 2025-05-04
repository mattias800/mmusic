import * as React from "react";
import { PropsWithChildren } from "react";

export interface PageCardProps extends PropsWithChildren {
  header?: React.ReactNode;
}

export const PageCard: React.FC<PageCardProps> = ({ children, header }) => {
  return (
    <div className={"rounded-2xl overflow-hidden bg-black"}>
      {header && (
        <div className="bg-gradient-to-b from-cyan-700 to-black text-white p-6">
          {header}
        </div>
      )}
      <div className={"px-8 pb-8"}>{children}</div>
    </div>
  );
};
