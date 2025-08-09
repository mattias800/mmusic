import * as React from "react";
import { Shuffle } from "lucide-react";

export type ShuffleButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement>;

export const ShuffleButton: React.FC<ShuffleButtonProps> = ({ ...props }) => {
  return (
    <button
      className="flex h-12 w-12 justify-center items-center text-white text-2xl opacity-70 hover:bg-green-900 cursor-pointer rounded-full"
      {...props}
    >
      <Shuffle />
    </button>
  );
};
