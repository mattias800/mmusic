import * as React from "react";
import { PropsWithChildren } from "react";
import { cn } from "@/lib/utils.ts";

export interface CardyProps extends PropsWithChildren {
  className?: string;
}

export const Cardy: React.FC<CardyProps> = ({ children, className }) => {
  return (
    <div
      className={cn(
        "bg-gray-900 px-6 py-8 shadow sm:rounded-lg sm:px-10 flex flex-col gap-4",
        className,
      )}
    >
      {children}
    </div>
  );
};
