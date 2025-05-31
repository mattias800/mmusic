import * as React from "react";
import { Spinner } from "@/components/spinner/Spinner.tsx";

export interface SectionHeadingProps {
  children: string | string[];
  loading?: boolean;
}

export const SectionHeading: React.FC<SectionHeadingProps> = ({
  children,
  loading,
}) => {
  return (
    <h2 className="text-xl font-semibold flex gap-2 items-center">
      {children} {loading && <Spinner size={"sm"} />}
    </h2>
  );
};
