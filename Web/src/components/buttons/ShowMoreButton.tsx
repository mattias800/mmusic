import * as React from "react";
import { Button, ButtonProps } from "../ui/button";

export interface ShowMoreButtonProps extends ButtonProps {}

export const ShowMoreButton: React.FC<ShowMoreButtonProps> = (props) => {
  return (
    <Button variant={"secondary"} {...props}>
      Show more
    </Button>
  );
};
