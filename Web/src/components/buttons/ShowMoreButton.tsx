import * as React from "react";
import {
  SecondaryButton,
  SecondaryButtonProps,
} from "@/components/buttons/core-buttons/SecondaryButton.tsx";

export interface ShowMoreButtonProps
  extends Omit<SecondaryButtonProps, "label"> {}

export const ShowMoreButton: React.FC<ShowMoreButtonProps> = (props) => {
  return <SecondaryButton {...props} label={"Show more"} />;
};
