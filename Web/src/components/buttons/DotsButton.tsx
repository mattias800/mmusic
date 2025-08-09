import * as React from "react";

export type DotsButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement>;

export const DotsButton: React.FC<DotsButtonProps> = ({ ...props }) => {
  return (
    <button
      className="flex h-12 w-12 justify-center items-center text-white text-2xl opacity-70 hover:bg-green-900 cursor-pointer rounded-full"
      {...props}
    >
      •••
    </button>
  );
};
