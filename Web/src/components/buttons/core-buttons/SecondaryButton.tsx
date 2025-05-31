import * as React from "react";
import { ComponentType } from "react";
import { LoaderCircle } from "lucide-react";

export interface SecondaryButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  label: string;
  loading?: boolean;
  iconLeft?: ComponentType<{ className: string }>;
  iconRight?: ComponentType<{ className: string }>;
}

export const SecondaryButton: React.FC<SecondaryButtonProps> = ({
  label,
  loading,
  disabled,
  iconLeft,
  iconRight,
  ...props
}) => {
  const isDisabled = disabled || loading;

  const IconLeft = iconLeft;
  const IconRight = iconRight;

  return (
    <button
      className={`border rounded-full px-4 py-1 text-sm transition flex items-center gap-2 
        ${
          isDisabled
            ? "border-gray-400 text-gray-400 cursor-not-allowed bg-transparent"
            : "border-white text-white hover:bg-white hover:text-black cursor-pointer"
        } ${iconLeft ? "pl-3" : ""} ${iconRight ? "pr-3" : ""}`}
      disabled={isDisabled}
      {...props}
    >
      {IconLeft && !loading ? <IconLeft className={"h-4"} /> : null}
      {loading && <LoaderCircle className={"animate-spin size-4"} />}
      {label}
      {IconRight && !loading ? <IconRight className={"h-4"} /> : null}
    </button>
  );
};
