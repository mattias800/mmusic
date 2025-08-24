import * as React from "react";
import { cn } from "@/lib/utils";

export interface SwitchProps {
  checked: boolean;
  onCheckedChange: (checked: boolean) => void;
  disabled?: boolean;
  id?: string;
  name?: string;
  className?: string;
  size?: "sm" | "md" | "lg";
  ariaLabel?: string;
}

export const Switch: React.FC<SwitchProps> = ({
  checked,
  onCheckedChange,
  disabled,
  id,
  name,
  className,
  size = "md",
  ariaLabel,
}) => {
  const sizes = {
    sm: {
      track: "h-5 w-9",
      thumb: "h-4 w-4 translate-x-0.5",
      shift: "translate-x-4",
    },
    md: {
      track: "h-6 w-11",
      thumb: "h-5 w-5 translate-x-0.5",
      shift: "translate-x-5",
    },
    lg: {
      track: "h-7 w-14",
      thumb: "h-6 w-6 translate-x-1",
      shift: "translate-x-7",
    },
  } as const;

  const { track, thumb, shift } = sizes[size];

  const toggle = React.useCallback(() => {
    if (disabled) return;
    onCheckedChange(!checked);
  }, [checked, disabled, onCheckedChange]);

  const onKeyDown = (e: React.KeyboardEvent<HTMLButtonElement>) => {
    if (e.key === " " || e.key === "Enter") {
      e.preventDefault();
      toggle();
    }
  };

  return (
    <button
      type="button"
      role="switch"
      aria-checked={checked}
      aria-label={ariaLabel}
      aria-disabled={disabled || undefined}
      id={id}
      name={name}
      disabled={disabled}
      onClick={toggle}
      onKeyDown={onKeyDown}
      className={cn(
        "relative inline-flex items-center rounded-full transition-colors outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 ring-offset-background",
        checked ? "bg-green-500" : "bg-gray-600",
        disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer",
        track,
        className,
      )}
    >
      <span
        aria-hidden
        className={cn(
          "pointer-events-none inline-block rounded-full bg-white shadow transition-transform",
          thumb,
          checked ? shift : "translate-x-0",
        )}
      />
    </button>
  );
};
