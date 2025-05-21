import * as React from "react";

export interface PhotoCardBottomTextProps {
  children: string;
}

export const PhotoCardBottomText: React.FC<PhotoCardBottomTextProps> = ({
  children,
}) => {
  return (
    <div className="absolute bottom-3 right-4 rounded text-xs text-white z-20">
      {children}
    </div>
  );
};
