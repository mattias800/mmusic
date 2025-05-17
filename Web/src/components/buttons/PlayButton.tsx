import * as React from "react";
import { Play, Pause, Download } from "lucide-react";

export type PlayButtonIconVariant = "play" | "pause" | "download";

export interface PlayButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  iconVariant?: PlayButtonIconVariant;
}

export const PlayButton: React.FC<PlayButtonProps> = ({
  iconVariant = "play",
  ...props
}) => {
  return (
    <button
      className="bg-green-500 hover:bg-green-600 text-black font-bold rounded-full text-sm h-6 w-6 justify-center items-center flex"
      {...props}
    >
      {iconVariant === "play" && <Play className="h-6 w-4" />}
      {iconVariant === "pause" && <Pause className="h-6 w-4" />}
      {iconVariant === "download" && <Download className="h-6 w-4" />}
    </button>
  );
};
