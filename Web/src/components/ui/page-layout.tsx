import React from "react";

export interface PageLayoutProps {
  children: React.ReactNode;
  containerClassName?: string;
}

export const PageLayout: React.FC<PageLayoutProps> = ({
  children,
  containerClassName = "",
}) => {
  return (
    <div className={`container mx-auto px-6 py-6 ${containerClassName}`}>
      {children}
    </div>
  );
};
