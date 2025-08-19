import React from "react";
import { LucideIcon } from "lucide-react";

export interface PageHeaderProps {
  icon: LucideIcon;
  title: string;
  subtitle?: string;
  avatarSize?: "sm" | "md" | "lg";
  className?: string;
}

export const PageHeader: React.FC<PageHeaderProps> = ({
  icon: Icon,
  title,
  subtitle,
  avatarSize = "lg",
  className = "",
}) => {
  const sizeClasses = {
    sm: "w-16 h-16",
    md: "w-20 h-20",
    lg: "w-24 h-24",
  };

  const iconSizeClasses = {
    sm: "w-8 h-8",
    md: "w-10 h-10",
    lg: "w-12 h-12",
  };

  return (
    <div className={`text-center my-12 ${className}`}>
      <div
        className={`inline-flex items-center justify-center ${sizeClasses[avatarSize]} bg-gradient-to-br from-blue-500 via-purple-500 to-pink-500 rounded-full mb-6 shadow-2xl`}
      >
        <Icon className={`${iconSizeClasses[avatarSize]} text-white`} />
      </div>
      <h1 className="text-4xl font-bold text-white mb-2">{title}</h1>
      {subtitle && <p className="text-gray-300 text-lg">{subtitle}</p>}
    </div>
  );
};
