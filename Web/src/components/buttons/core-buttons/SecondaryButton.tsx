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
  ...props
}) => {
  return (
    <button
      className="border border-white rounded-full px-4 py-1 text-sm hover:bg-white hover:text-black transition flex items-center gap-2 cursor-pointer"
      {...props}
    >
      {loading && <LoaderCircle className={"animate-spin size-4"} />}
      {label}
    </button>
  );
};
