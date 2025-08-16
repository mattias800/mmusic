import React from "react";
import { LucideIcon } from "lucide-react";

export interface StatusCardProps {
  label: string;
  value: React.ReactNode;
  icon?: LucideIcon;
  className?: string;
}

export const StatusCard: React.FC<StatusCardProps> = ({
  label,
  value,
  icon: Icon,
  className = ""
}) => {
  return (
    <div className={`p-4 bg-white/5 rounded-lg border border-white/10 ${className}`}>
      <div className="flex items-center gap-2 mb-2">
        {Icon && <Icon className="w-4 h-4 text-gray-400" />}
        <span className="text-gray-400 text-sm font-medium">{label}</span>
      </div>
      <div className="text-white font-medium">
        {value}
      </div>
    </div>
  );
};

export interface StatusGridProps {
  children: React.ReactNode;
  columns?: 1 | 2 | 3 | 4;
  className?: string;
}

export const StatusGrid: React.FC<StatusGridProps> = ({
  children,
  columns = 2,
  className = ""
}) => {
  const gridClasses = {
    1: "grid-cols-1",
    2: "grid-cols-1 md:grid-cols-2",
    3: "grid-cols-1 md:grid-cols-2 lg:grid-cols-3",
    4: "grid-cols-1 md:grid-cols-2 lg:grid-cols-4"
  };

  return (
    <div className={`grid ${gridClasses[columns]} gap-4 ${className}`}>
      {children}
    </div>
  );
};
