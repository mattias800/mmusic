import * as React from "react";
import { Play, Pause, Download, Search } from "lucide-react";

export type PlayButtonIconVariant = "play" | "pause" | "download" | "search";

export interface PlayButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  iconVariant?: PlayButtonIconVariant;
}

export const PlayButton: React.FC<PlayButtonProps> = ({
  iconVariant = "play",
  ...props
}) => {
  return (
    <button
      className="bg-green-500 hover:bg-green-600 text-black font-bold rounded-full text-sm h-6 w-6 justify-center items-center flex cursor-pointer"
      {...props}
    >
      {iconVariant === "search" && <Search className="h-6 w-4" />}
      {iconVariant === "play" && <Play className="h-6 w-4" />}
      {iconVariant === "pause" && <Pause className="h-6 w-4" />}
      {iconVariant === "download" && <Download className="h-6 w-4" />}
    </button>
  );
};
