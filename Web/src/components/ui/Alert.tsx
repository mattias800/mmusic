import * as React from "react";
import { cn } from "@/lib/utils";

export interface AlertProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: "default" | "success" | "warning" | "error";
  title?: string;
}

export function Alert({
  className,
  variant = "default",
  title,
  children,
  ...props
}: AlertProps) {
  const variantClasses = {
    default: "bg-background border-muted text-foreground",
    success: "bg-green-50 border-green-200 text-green-800 dark:bg-green-950 dark:border-green-800 dark:text-green-300",
    warning: "bg-yellow-50 border-yellow-200 text-yellow-800 dark:bg-yellow-950 dark:border-yellow-800 dark:text-yellow-300",
    error: "bg-red-50 border-red-200 text-red-800 dark:bg-red-950 dark:border-red-800 dark:text-red-300",
  };

  return (
    <div
      className={cn(
        "rounded-md border p-4",
        variantClasses[variant],
        className
      )}
      role="alert"
      {...props}
    >
      {title && <h5 className="mb-2 font-medium">{title}</h5>}
      <div className="text-sm">{children}</div>
    </div>
  );
}
