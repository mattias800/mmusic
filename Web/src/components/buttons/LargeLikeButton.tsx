import * as React from "react";
import { Plus } from "lucide-react";

export interface LargeLikeButtonProps {}

export const LargeLikeButton: React.FC<LargeLikeButtonProps> = () => {
  return (
    <button className="text-white opacity-70 hover:opacity-100 text-xl">
      <Plus />
    </button>
  );
};
