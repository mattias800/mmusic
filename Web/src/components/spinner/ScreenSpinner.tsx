import * as React from "react";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";

export interface ScreenSpinnerProps {}

export const ScreenSpinner: React.FC<ScreenSpinnerProps> = () => {
  return (
    <SpinnerSpacing>
      <Spinner />
    </SpinnerSpacing>
  );
};
