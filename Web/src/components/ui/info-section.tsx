import React from "react";
import { LucideIcon } from "lucide-react";

export interface InfoSectionProps {
  icon: LucideIcon;
  title: string;
  children: React.ReactNode;
  variant?: "default" | "blue" | "green" | "purple";
  className?: string;
}

export const InfoSection: React.FC<InfoSectionProps> = ({
  icon: Icon,
  title,
  children,
  variant = "default",
  className = ""
}) => {
  const variantClasses = {
    default: "from-gray-500/10 to-gray-600/10 border-gray-500/20",
    blue: "from-blue-500/10 to-purple-500/10 border-blue-500/20",
    green: "from-green-500/10 to-emerald-500/10 border-green-500/20",
    purple: "from-purple-500/10 to-pink-500/10 border-purple-500/20"
  };

  const iconColors = {
    default: "text-gray-400",
    blue: "text-blue-400",
    green: "text-green-400",
    purple: "text-purple-400"
  };

  return (
    <div className={`mt-12 max-w-4xl mx-auto ${className}`}>
      <div className={`bg-gradient-to-r ${variantClasses[variant]} border rounded-2xl p-8 text-center`}>
        <div className="flex items-center justify-center gap-3 mb-4">
          <Icon className={`w-6 h-6 ${iconColors[variant]}`} />
          <h3 className="text-xl font-semibold text-white">{title}</h3>
        </div>
        <div className="text-gray-300 leading-relaxed">
          {children}
        </div>
      </div>
    </div>
  );
};
