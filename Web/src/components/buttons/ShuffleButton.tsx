import * as React from "react";
import { Shuffle } from "lucide-react";

export interface ShuffleButtonProps {}

export const ShuffleButton: React.FC<ShuffleButtonProps> = () => {
  return (
    <button className="text-white opacity-70 hover:opacity-100 text-xl">
      <Shuffle />
    </button>
  );
};
