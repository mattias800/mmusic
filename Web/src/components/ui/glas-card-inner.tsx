import * as React from "react";
import { PropsWithChildren } from "react";

export interface GlassCardInnerProps extends PropsWithChildren {
  icon?: React.ReactNode;
  heading?: string;
}

export const GlassCardInner: React.FC<GlassCardInnerProps> = ({
  icon,
  heading,
  children,
}) => {
  return (
    <div className="p-4 bg-white/5 rounded-lg border border-white/10">
      {heading && (
        <div className="flex items-center gap-3 mb-3">
          {icon && (
            <div className="p-2 bg-purple-500/20 rounded-lg">{icon}</div>
          )}
          <h3 className="text-lg font-semibold text-white">{heading}</h3>
        </div>
      )}
      {children}
    </div>
  );
};
