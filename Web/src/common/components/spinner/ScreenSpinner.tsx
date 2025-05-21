import * as React from "react";
import { SpinnerSpacing } from "@/common/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/common/components/spinner/Spinner.tsx";

export interface ScreenSpinnerProps {}

export const ScreenSpinner: React.FC<ScreenSpinnerProps> = () => {
  return (
    <SpinnerSpacing>
      <Spinner />
    </SpinnerSpacing>
  );
};
