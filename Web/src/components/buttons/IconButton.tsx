import * as React from "react";

export type IconButtonVariant = "primary" | "flat";
export interface IconButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: IconButtonVariant;
}

export const IconButton: React.FC<IconButtonProps> = ({
  variant,
  ...props
}) => {
  return (
    <button
      className={`${variant === "primary" ? "bg-green-500 hover:bg-green-600 text-black" : "bg-green-500 hover:bg-green-600 text-black"} font-bold rounded-full text-sm h-6 w-6 justify-center items-center flex cursor-pointer`}
      {...props}
    />
  );
};
