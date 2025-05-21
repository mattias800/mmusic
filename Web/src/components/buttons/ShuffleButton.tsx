import * as React from "react";
import { Shuffle } from "lucide-react";

export type ShuffleButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement>;

export const ShuffleButton: React.FC<ShuffleButtonProps> = ({ ...props }) => {
  return (
    <button
      className="text-white opacity-70 hover:opacity-100 text-xl"
      {...props}
    >
      <Shuffle />
    </button>
  );
};
