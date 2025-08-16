import React from "react";

export interface PageLayoutProps {
  children: React.ReactNode;
  className?: string;
  containerClassName?: string;
}

export const PageLayout: React.FC<PageLayoutProps> = ({
  children,
  className = "",
  containerClassName = ""
}) => {
  return (
    <div className={`min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 ${className}`}>
      <div className={`container mx-auto px-4 py-8 ${containerClassName}`}>
        {children}
      </div>
    </div>
  );
};
