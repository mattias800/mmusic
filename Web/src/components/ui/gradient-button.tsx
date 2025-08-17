import React from "react";
import { ButtonHTMLAttributes } from "react";
import { cn } from "@/lib/utils";
import { LoaderCircle } from "lucide-react";

export interface GradientButtonProps
  extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: "primary" | "secondary" | "success" | "danger";
  size?: "sm" | "md" | "lg";
  fullWidth?: boolean;
  children: React.ReactNode;
  loading?: boolean;
  iconLeft?: React.ComponentType<{ className?: string }>;
}

export const GradientButton: React.FC<GradientButtonProps> = ({
  variant = "primary",
  size = "md",
  fullWidth = false,
  className = "",
  children,
  loading,
  iconLeft: IconLeft,
  ...props
}) => {
  const variantClasses = {
    primary:
      "bg-gradient-to-r from-blue-500 to-purple-500 hover:from-blue-600 hover:to-purple-600",
    secondary:
      "bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-600 hover:to-gray-700",
    success:
      "bg-gradient-to-r from-green-500 to-emerald-500 hover:from-green-600 hover:to-emerald-600",
    danger:
      "bg-gradient-to-r from-red-500 to-pink-500 hover:from-red-600 hover:to-pink-600",
  };

  const sizeClasses = {
    sm: "px-3 py-2 text-sm",
    md: "px-4 py-2 text-base",
    lg: "px-6 py-3 text-lg",
  };

  return (
    <button
      className={cn(
        "inline-flex items-center justify-center font-medium text-white border-0 shadow-lg rounded-lg transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-gray-900 focus:ring-blue-400 disabled:opacity-50 disabled:cursor-not-allowed",
        variantClasses[variant],
        sizeClasses[size],
        fullWidth && "w-full",
        className,
      )}
      {...props}
    >
      {loading ? (
        <LoaderCircle className="animate-spin size-4 mr-2" />
      ) : (
        IconLeft ? <IconLeft className="w-4 h-4 mr-2" /> : null
      )}
      {children}
    </button>
  );
};
