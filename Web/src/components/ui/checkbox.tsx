import * as React from "react";
import { cn } from "@/lib/utils";

export interface CheckboxProps
  extends Omit<
    React.InputHTMLAttributes<HTMLInputElement>,
    "onChange" | "type"
  > {
  checked: boolean;
  onCheckedChange: (checked: boolean) => void;
  label?: React.ReactNode;
}

export const Checkbox: React.FC<CheckboxProps> = ({
  checked,
  onCheckedChange,
  disabled,
  id,
  name,
  className,
  label,
  ...rest
}) => {
  return (
    <label
      className={cn(
        "inline-flex items-center gap-2 select-none",
        disabled && "opacity-50 cursor-not-allowed",
      )}
    >
      <input
        type="checkbox"
        id={id}
        name={name}
        checked={checked}
        onChange={(e) => onCheckedChange(e.target.checked)}
        disabled={disabled}
        className={cn(
          "peer appearance-none shrink-0 w-4 h-4 rounded border border-gray-500 bg-transparent transition-colors",
          "focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-green-500",
          "checked:bg-green-500 checked:border-green-500",
          className,
        )}
        {...rest}
      />
      <span className="pointer-events-none inline-flex items-center justify-center w-4 h-4 -ml-4 text-black">
        {/* checkmark via pseudo-element using peer-checked */}
        <svg
          className="opacity-0 peer-checked:opacity-100 transition-opacity"
          width="12"
          height="12"
          viewBox="0 0 12 12"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
          aria-hidden
        >
          <path
            d="M2 6.5L4.5 9L10 3.5"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      </span>
      {label ? <span className="text-sm text-gray-200">{label}</span> : null}
    </label>
  );
};
