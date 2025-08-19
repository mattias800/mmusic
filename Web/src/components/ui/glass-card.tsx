import React from "react";
import { LucideIcon } from "lucide-react";

export interface GlassCardProps {
  children: React.ReactNode;
  title?: string;
  icon?: LucideIcon;
  iconBgColor?: string;
  className?: string;
  padding?: "sm" | "md" | "lg";
}

export const GlassCard: React.FC<GlassCardProps> = ({
  children,
  title,
  icon: Icon,
  iconBgColor = "bg-blue-500/20",
  className = "",
  padding = "lg",
}) => {
  const paddingClasses = {
    sm: "p-4",
    md: "p-6",
    lg: "p-8",
  };

  return (
    <div
      className={`bg-white/5 backdrop-blur-sm border border-white/10 rounded-2xl ${paddingClasses[padding]} shadow-2xl ${className}`}
    >
      {title && (
        <div className="flex items-center gap-3 mb-6">
          {Icon && (
            <div className={`p-2 ${iconBgColor} rounded-lg`}>
              <Icon className="w-5 h-5 text-blue-400" />
            </div>
          )}
          <h2 className="text-xl font-semibold text-white">{title}</h2>
        </div>
      )}
      {children}
    </div>
  );
};
