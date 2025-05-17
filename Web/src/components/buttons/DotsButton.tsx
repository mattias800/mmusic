import * as React from "react";

export type DotsButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement>;

export const DotsButton: React.FC<DotsButtonProps> = ({ ...props }) => {
  return (
    <button
      className="text-white text-2xl opacity-70 hover:opacity-100"
      {...props}
    >
      •••
    </button>
  );
};
