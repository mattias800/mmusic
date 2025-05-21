import * as React from "react";
import { Plus } from "lucide-react";

export type LargeLikeButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement>;

export const LargeLikeButton: React.FC<LargeLikeButtonProps> = ({ ...props }) => {
  return (
    <button
      className="text-white opacity-70 hover:opacity-100 text-xl"
      {...props}
    >
      <Plus />
    </button>
  );
};
