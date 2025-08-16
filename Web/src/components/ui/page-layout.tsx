import React from "react";

export interface PageLayoutProps {
  children: React.ReactNode;
  containerClassName?: string;
  addSearchPadding?: boolean;
}

export const PageLayout: React.FC<PageLayoutProps> = ({
  children,
  containerClassName = "",
  addSearchPadding,
}) => {
  return (
    <div
      className={`container mx-auto px-6 py-6 ${containerClassName} min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 ${addSearchPadding ? "pt-20" : ""}`}
    >
      {children}
    </div>
  );
};
