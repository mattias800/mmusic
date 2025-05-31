import * as React from "react";
import { cn } from "@/lib/utils.ts";

export interface PrimaryButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  label: string;
  loading?: boolean;
  loadingLabel?: string;
}

export const PrimaryButton: React.FC<PrimaryButtonProps> = ({
  label,
  loading,
  loadingLabel,
  className,
  ...props
}) => {
  return (
    <button
      {...props}
      type="submit"
      className={cn(
        className,
        "w-full flex justify-center rounded-md px-3 py-2.5 text-sm font-semibold leading-6 shadow-sm",
        loading
          ? "bg-gray-500 hover:bg-gray-500 cursor-not-allowed"
          : "bg-indigo-600 hover:bg-indigo-500 focus-visible:outline focus-visible:outline-offset-2 focus-visible:outline-indigo-600 text-white",
      )}
      disabled={loading}
    >
      {loading ? (loadingLabel ?? label) : label}
    </button>
  );
};
