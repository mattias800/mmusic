import * as React from "react";
import { Download, Pause, Play } from "lucide-react";

export type LargePlayButtonIconVariant = "play" | "pause" | "download";

export interface LargePlayButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  iconVariant?: LargePlayButtonIconVariant;
}

export const LargePlayButton: React.FC<LargePlayButtonProps> = ({
  iconVariant = "play",
  ...props
}) => {
  return (
    <button
      className="bg-green-500 hover:bg-green-600 text-black font-bold rounded-full text-sm h-14 w-14 justify-center items-center flex"
      {...props}
    >
      {iconVariant === "play" && <Play className="h-6 w-6" />}
      {iconVariant === "pause" && <Pause className="h-6 w-6" />}
      {iconVariant === "download" && <Download className="h-6 w-6" />}
    </button>
  );
};
