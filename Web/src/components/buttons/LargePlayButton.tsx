import * as React from "react";
import { Play, Pause, Download } from "lucide-react";

export type LargePlayButtonIconVariant = "play" | "pause" | "download";

export interface LargePlayButtonProps {
  iconVariant?: LargePlayButtonIconVariant;
}

export const LargePlayButton: React.FC<LargePlayButtonProps> = ({
  iconVariant = "play",
}) => {
  return (
    <button className="bg-green-500 hover:bg-green-600 text-black font-bold rounded-full text-sm h-14 w-14 justify-center items-center flex">
      {iconVariant === "play" && <Play className="h-6 w-6" />}
      {iconVariant === "pause" && <Pause className="h-6 w-6" />}
      {iconVariant === "download" && <Download className="h-6 w-6" />}
    </button>
  );
};
