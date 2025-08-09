import * as React from "react";
import { Plus } from "lucide-react";

export type LargeLikeButtonProps =
  React.ButtonHTMLAttributes<HTMLButtonElement>;

export const LargeLikeButton: React.FC<LargeLikeButtonProps> = ({
  ...props
}) => {
  return (
    <button
      className="flex h-12 w-12 justify-center items-center text-white text-2xl opacity-70 hover:bg-green-900 cursor-pointer rounded-full"
      {...props}
    >
      <Plus />
    </button>
  );
};
