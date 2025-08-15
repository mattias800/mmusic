import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { 
  Disc3, 
  Music, 
  Album, 
  BarChart3, 
  CheckCircle2, 
  AlertCircle 
} from "lucide-react";

export interface ArtistStatisticsHeaderProps {
  artist: FragmentType<typeof artistStatisticsHeaderArtistFragment>;
}

const artistStatisticsHeaderArtistFragment = graphql(`
  fragment ArtistStatisticsHeader_Artist on Artist {
    id
    name
    albums {
      id
      isFullyMissing
    }
    eps {
      id
      isFullyMissing
    }
    singles {
      id
      isFullyMissing
    }
  }
`);

export const ArtistStatisticsHeader: React.FC<ArtistStatisticsHeaderProps> = (props) => {
  const artist = useFragment(artistStatisticsHeaderArtistFragment, props.artist);

  // Calculate statistics
  const totalAlbums = artist.albums?.length ?? 0;
  const availableAlbums = artist.albums?.filter(r => !r.isFullyMissing).length ?? 0;
  const albumProgress = totalAlbums > 0 ? (availableAlbums / totalAlbums) * 100 : 0;

  const totalEps = artist.eps?.length ?? 0;
  const availableEps = artist.eps?.filter(r => !r.isFullyMissing).length ?? 0;
  const epProgress = totalEps > 0 ? (availableEps / totalEps) * 100 : 0;

  const totalSingles = artist.singles?.length ?? 0;
  const availableSingles = artist.singles?.filter(r => !r.isFullyMissing).length ?? 0;
  const singleProgress = totalSingles > 0 ? (availableSingles / totalSingles) * 100 : 0;

  const totalReleases = totalAlbums + totalEps + totalSingles;
  const availableReleases = availableAlbums + availableEps + availableSingles;
  const overallProgress = totalReleases > 0 ? (availableReleases / totalReleases) * 100 : 0;

  // Helper function to render progress bar
  const renderProgressBar = (progress: number, total: number, available: number, color: string) => {
    if (total === 0) return null;
    
    return (
      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between text-sm mb-1">
          <span className="text-zinc-300 font-medium">
            {available} / {total}
          </span>
          <span className="text-zinc-400 text-xs">
            {Math.round(progress)}%
          </span>
        </div>
        <div className="w-full bg-zinc-700 rounded-full h-2">
          <div 
            className={`h-2 rounded-full transition-all duration-300 ${color}`}
            style={{ width: `${progress}%` }}
          />
        </div>
      </div>
    );
  };

  // Helper function to render release type card
  const renderReleaseTypeCard = (
    icon: React.ReactNode,
    title: string,
    total: number,
    available: number,
    progress: number,
    color: string
  ) => {
    if (total === 0) return null;

    return (
      <div className="bg-zinc-800/50 border border-zinc-700 rounded-lg p-4 flex-1 min-w-0">
        <div className="flex items-center gap-3 mb-3">
          <div className="p-2 bg-zinc-700 rounded-lg">
            {icon}
          </div>
          <div>
            <h3 className="font-semibold text-zinc-200">{title}</h3>
            <p className="text-sm text-zinc-400">
              {available} of {total} available
            </p>
          </div>
        </div>
        {renderProgressBar(progress, total, available, color)}
      </div>
    );
  };

  return (
    <div className="px-6 md:px-10 py-6 bg-gradient-to-r from-zinc-900/50 to-zinc-800/50 border-b border-zinc-700">
      {/* Overall Statistics Header */}
      <div className="flex items-center gap-3 mb-6">
        <div className="p-3 bg-purple-600/20 border border-purple-500/30 rounded-lg">
          <BarChart3 className="w-6 h-6 text-purple-400" />
        </div>
        <div>
          <h2 className="text-xl font-bold text-zinc-100">Media Availability</h2>
          <p className="text-zinc-400">
            {availableReleases} of {totalReleases} releases have complete media files
          </p>
        </div>
      </div>

      {/* Overall Progress Bar */}
      {totalReleases > 0 && (
        <div className="mb-6">
          <div className="flex items-center justify-between text-sm mb-2">
            <span className="text-zinc-300 font-medium">Overall Progress</span>
            <span className="text-zinc-400">
              {availableReleases} / {totalReleases} releases
            </span>
          </div>
          <div className="w-full bg-zinc-700 rounded-full h-3">
            <div 
              className="h-3 rounded-full bg-gradient-to-r from-green-500 to-blue-500 transition-all duration-500"
              style={{ width: `${overallProgress}%` }}
            />
          </div>
          <div className="flex items-center gap-2 mt-2">
            {overallProgress === 100 ? (
              <CheckCircle2 className="w-4 h-4 text-green-400" />
            ) : (
              <AlertCircle className="w-4 h-4 text-yellow-400" />
            )}
            <span className="text-sm text-zinc-400">
              {overallProgress === 100 
                ? "All releases have complete media files!" 
                : `${totalReleases - availableReleases} releases need media files`
              }
            </span>
          </div>
        </div>
      )}

      {/* Release Type Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {renderReleaseTypeCard(
          <Album className="w-5 h-5 text-blue-400" />,
          "Albums",
          totalAlbums,
          availableAlbums,
          albumProgress,
          "bg-blue-500"
        )}
        
        {renderReleaseTypeCard(
          <Disc3 className="w-5 h-5 text-purple-400" />,
          "EPs",
          totalEps,
          availableEps,
          epProgress,
          "bg-purple-500"
        )}
        
        {renderReleaseTypeCard(
          <Music className="w-5 h-5 text-green-400" />,
          "Singles",
          totalSingles,
          availableSingles,
          singleProgress,
          "bg-green-500"
        )}
      </div>

      {/* Quick Stats */}
      <div className="mt-6 grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
        <div className="bg-zinc-800/30 border border-zinc-700 rounded-lg p-3">
          <div className="text-2xl font-bold text-zinc-100">{totalReleases}</div>
          <div className="text-xs text-zinc-400">Total Releases</div>
        </div>
        
        <div className="bg-zinc-800/30 border border-zinc-700 rounded-lg p-3">
          <div className="text-2xl font-bold text-green-400">{availableReleases}</div>
          <div className="text-xs text-zinc-400">Available</div>
        </div>
        
        <div className="bg-zinc-800/30 border border-zinc-700 rounded-lg p-3">
          <div className="text-2xl font-bold text-yellow-400">{totalReleases - availableReleases}</div>
          <div className="text-xs text-zinc-400">Missing</div>
        </div>
        
        <div className="bg-zinc-800/30 border border-zinc-700 rounded-lg p-3">
          <div className="text-2xl font-bold text-blue-400">{Math.round(overallProgress)}%</div>
          <div className="text-xs text-zinc-400">Complete</div>
        </div>
      </div>
    </div>
  );
};
