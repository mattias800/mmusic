import * as React from "react";
import { PropsWithChildren } from "react";

export type TagVariant = "info" | "error" | "success" | "warning";

export interface TagProps extends PropsWithChildren {
  variant?: TagVariant;
}

const variantClasses: Record<TagVariant, { bg: string; text: string }> = {
  info: {
    bg: "bg-blue-600/40",
    text: "text-blue-100",
  },
  error: {
    bg: "bg-red-600/30",
    text: "text-red-300",
  },
  success: {
    bg: "bg-green-600/40",
    text: "text-green-100",
  },
  warning: {
    bg: "bg-yellow-600/40",
    text: "text-yellow-100",
  },
};

export const Tag: React.FC<TagProps> = ({ children, variant = "info" }) => {
  const { bg, text } = variantClasses[variant];

  return (
    <span
      className={`inline-flex items-center rounded ${bg} px-2 py-[2px] text-xs ${text}`}
    >
      {children}
    </span>
  );
};
