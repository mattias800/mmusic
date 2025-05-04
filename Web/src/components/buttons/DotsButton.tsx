import * as React from "react";

export interface DotsButtonProps {}

export const DotsButton: React.FC<DotsButtonProps> = () => {
  return (
    <button className="text-white text-2xl opacity-70 hover:opacity-100">
      •••
    </button>
  );
};
