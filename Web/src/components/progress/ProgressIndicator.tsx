import * as React from "react";

export interface ProgressIndicatorProps {
  progressPercent: number;
}

export const ProgressIndicator: React.FC<ProgressIndicatorProps> = ({
  progressPercent,
}) => {
  return (
    <div className="w-full bg-zinc-700 rounded-full h-2.5 overflow-hidden">
      <div
        className="bg-green-500 h-full transition-all duration-300"
        style={{ width: `${progressPercent}%` }}
      />
    </div>
  );
};
