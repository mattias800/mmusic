import * as React from "react";
import { AlertTriangle } from "lucide-react";
import { ArtistInLibraryButton } from "@/features/add-artist-to-server-library/ArtistInLibraryButton.tsx";

export interface AddArtistToLibraryBoxProps {
  artistId: string;
  artistName: string;
  isInLibrary: boolean;
}

export const AddArtistToLibraryBox: React.FC<AddArtistToLibraryBoxProps> = ({
  artistName,
  artistId,
  isInLibrary,
}) => {
  return (
    <div className="relative group">
      {/* Beautiful Background */}
      <div className="absolute inset-0 bg-gradient-to-br from-orange-500/10 to-red-500/10 rounded-2xl border border-orange-500/20 opacity-0 " />

      {/* Content */}
      <div className="relative p-8 bg-gradient-to-br from-orange-500/5 to-red-500/5 rounded-2xl border border-orange-500/20 backdrop-blur-sm transition-all duration-300 group-hover:border-orange-500/30 group-hover:shadow-lg">
        <div className="text-center space-y-6">
          {/* Icon and Status */}
          <div className="inline-flex items-center justify-center w-20 h-20 bg-gradient-to-br from-orange-500/20 to-red-500/20 rounded-2xl border border-orange-500/30 mb-4">
            <AlertTriangle className="w-10 h-10 text-orange-400" />
          </div>

          <div className="space-y-3">
            <h3 className="text-2xl font-bold text-white">Not in Library</h3>
            <p className="text-gray-300 text-lg leading-relaxed">
              <span className="font-semibold text-white">{artistName}</span> is
              not in your music library yet
            </p>
            <p className="text-gray-400 text-sm">
              Click below to import and start building your collection
            </p>
          </div>

          {/* Import Button */}
          <div className="pt-4">
            <div className="mt-4 flex justify-center">
              <ArtistInLibraryButton
                artistId={artistId}
                isInLibrary={isInLibrary}
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
