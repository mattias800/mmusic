import * as React from "react";
import {
  Download,
  Pause,
  Play,
  Search,
  StepBack,
  StepForward,
} from "lucide-react";
import { IconButton } from "@/components/buttons/IconButton.tsx";

export type PlayButtonIconVariant =
  | "play"
  | "pause"
  | "download"
  | "search"
  | "prev"
  | "next";

export interface PlayButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  iconVariant?: PlayButtonIconVariant;
}

export const PlayButton: React.FC<PlayButtonProps> = ({
  iconVariant = "play",
  ...props
}) => {
  return (
    <IconButton {...props}>
      {iconVariant === "search" && <Search className="h-6 w-4" />}
      {iconVariant === "play" && <Play className="h-6 w-4" />}
      {iconVariant === "pause" && <Pause className="h-6 w-4" />}
      {iconVariant === "next" && <StepForward className="h-6 w-4" />}
      {iconVariant === "prev" && <StepBack className="h-6 w-4" />}
      {iconVariant === "download" && <Download className="h-6 w-4" />}
    </IconButton>
  );
};
