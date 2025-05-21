import * as React from "react";
import { PropsWithChildren } from "react";

export interface GradientContentProps extends PropsWithChildren {}

export const GradientContent: React.FC<GradientContentProps> = ({
  children,
}) => {
  return (
    <div className="bg-gradient-to-b from-neutral-800 to-black text-white min-h-screen pb-12">
      {children}
    </div>
  );
};
