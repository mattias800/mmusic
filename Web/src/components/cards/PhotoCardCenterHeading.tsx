import * as React from "react";

export interface PhotoCardCenterHeadingProps {
  children: string;
}

export const PhotoCardCenterHeading: React.FC<PhotoCardCenterHeadingProps> = ({
  children,
}) => {
  return (
    <>
      <div className="absolute top-0 bottom-0 left-0 right-0 bg-gradient-to-b from-transparent via-black/60 to-transparent"></div>
      <div className="absolute inset-0 flex flex-col items-center justify-center p-4 z-20">
        <h3 className="text-white text-xl font-bold text-center drop-shadow-md">
          {children}
        </h3>
      </div>
    </>
  );
};
