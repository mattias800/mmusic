import * as React from "react";

export interface ErrorBoxProps {
  message: string;
}

export const ErrorBox: React.FC<ErrorBoxProps> = ({ message }) => {
  return (
    <div className="p-3 rounded-md bg-red-900/30 border border-red-700">
      <p className="text-sm text-red-400">{message}</p>
    </div>
  );
};
