import * as React from "react";
import { PropsWithChildren } from "react";

export interface PhotoCardProps extends PropsWithChildren {
  imageUrl: string;
  imageAlt?: string;
  onClick?: () => void;
}

export const PhotoCard: React.FC<PhotoCardProps> = ({
  imageUrl,
  imageAlt = "",
  onClick,
  children,
}) => {
  return (
    <button
      className="relative w-48 h-48 rounded-lg overflow-hidden shadow-lg group cursor-pointer"
      onClick={onClick}
    >
      {imageUrl ? (
        <img
          src={imageUrl}
          alt={imageAlt}
          className="absolute inset-0 w-full h-full object-cover transition-transform duration-300 ease-in-out group-hover:scale-110 z-0"
        />
      ) : (
        <div className="absolute inset-0 w-full h-full bg-gray-700 flex items-center justify-center z-0">
          <span className="text-gray-400 text-sm">No Image</span>
        </div>
      )}
      {children}
    </button>
  );
};
