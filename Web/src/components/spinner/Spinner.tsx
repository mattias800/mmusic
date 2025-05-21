import * as React from "react";
import { LoaderCircle } from "lucide-react";

export interface SpinnerProps {
  size?: "sm" | "md";
}

export const Spinner: React.FC<SpinnerProps> = ({ size = "md" }) => {
  switch (size) {
    case "sm":
      return <LoaderCircle className={"animate-spin"} />;
    case "md":
      return <LoaderCircle className={"size-12 animate-spin"} />;
    default:
      return <LoaderCircle className={"size-12 animate-spin"} />;
  }
};
