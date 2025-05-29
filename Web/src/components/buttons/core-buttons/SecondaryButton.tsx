import * as React from "react";
import { LoaderCircle } from "lucide-react";

export interface SecondaryButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  label: string;
  loading?: boolean;
}

export const SecondaryButton: React.FC<SecondaryButtonProps> = ({
  label,
  loading,
  disabled,
  ...props
}) => {
  const isDisabled = disabled || loading;

  return (
    <button
      className={`border rounded-full px-4 py-1 text-sm transition flex items-center gap-2 
        ${
          isDisabled
            ? "border-gray-400 text-gray-400 cursor-not-allowed bg-transparent"
            : "border-white text-white hover:bg-white hover:text-black cursor-pointer"
        }`}
      disabled={isDisabled}
      {...props}
    >
      {loading && <LoaderCircle className={"animate-spin size-4"} />}
      {label}
    </button>
  );
};
