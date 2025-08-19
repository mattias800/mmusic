import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import {
  Disc3,
  Music,
  Album,
  BarChart3,
  CheckCircle2,
  AlertCircle,
  Database,
  Clock,
  Users,
} from "lucide-react";
import { StatusCard, StatusGrid } from "@/components/ui";

export interface ServerLibraryStatisticsHeaderProps {
  serverLibrary: FragmentType<typeof serverLibraryStatisticsHeaderFragment>;
}

const serverLibraryStatisticsHeaderFragment = graphql(`
  fragment ServerLibraryStatisticsHeader_ServerLibrary on ServerLibrarySearchRoot {
    libraryStatistics {
      artistCount
      releaseCount
      trackCount
      isInitialized
      lastUpdated
    }
    allReleases {
      id
      type
      isFullyMissing
    }
  }
`);

export const ServerLibraryStatisticsHeader: React.FC<
  ServerLibraryStatisticsHeaderProps
> = (props) => {
  const serverLibrary = useFragment(
    serverLibraryStatisticsHeaderFragment,
    props.serverLibrary,
  );
  const stats = serverLibrary.libraryStatistics;
  const releases = serverLibrary.allReleases;

  // Calculate statistics
  const totalReleases = stats.releaseCount;
  const totalArtists = stats.artistCount;
  const totalTracks = stats.trackCount;

  // Calculate media availability from releases
  const releasesWithMedia = releases.filter((r) => !r.isFullyMissing).length;
  const releasesWithoutMedia = totalReleases - releasesWithMedia;
  const mediaProgress =
    totalReleases > 0 ? (releasesWithMedia / totalReleases) * 100 : 0;

  // Calculate release type breakdown
  const albums = releases.filter((r) => r.type === "ALBUM");
  const eps = releases.filter((r) => r.type === "EP");
  const singles = releases.filter((r) => r.type === "SINGLE");

  const albumsWithMedia = albums.filter((r) => !r.isFullyMissing).length;
  const epsWithMedia = eps.filter((r) => !r.isFullyMissing).length;
  const singlesWithMedia = singles.filter((r) => !r.isFullyMissing).length;

  const albumProgress =
    albums.length > 0 ? (albumsWithMedia / albums.length) * 100 : 0;
  const epProgress = eps.length > 0 ? (epsWithMedia / eps.length) * 100 : 0;
  const singleProgress =
    singles.length > 0 ? (singlesWithMedia / singles.length) * 100 : 0;

  // Format last updated time
  const formatLastUpdated = (dateTime: string) => {
    const date = new Date(dateTime);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / (1000 * 60));
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    if (diffMins < 1) return "Just now";
    if (diffMins < 60)
      return `${diffMins} minute${diffMins !== 1 ? "s" : ""} ago`;
    if (diffHours < 24)
      return `${diffHours} hour${diffHours !== 1 ? "s" : ""} ago`;
    return `${diffDays} day${diffDays !== 1 ? "s" : ""} ago`;
  };

  // Helper function to render progress bar
  const renderProgressBar = (
    progress: number,
    total: number,
    available: number,
    color: string,
  ) => {
    if (total === 0) return null;

    return (
      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between text-sm mb-2">
          <span className="text-white font-medium">
            {available} / {total}
          </span>
          <span className="text-gray-400 text-xs">{Math.round(progress)}%</span>
        </div>
        <div className="w-full bg-white/10 rounded-full h-2">
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
    color: string,
  ) => {
    if (total === 0) return null;

    return (
      <div className="p-4 bg-white/5 rounded-lg border border-white/10 flex-1 min-w-0">
        <div className="flex items-center gap-3 mb-3">
          <div className="p-2 bg-white/10 rounded-lg">{icon}</div>
          <div>
            <h3 className="font-semibold text-white">{title}</h3>
            <p className="text-sm text-gray-300">
              {available} of {total} available
            </p>
          </div>
        </div>
        {renderProgressBar(progress, total, available, color)}
      </div>
    );
  };

  if (!stats.isInitialized) {
    return (
      <div className="max-w-7xl mx-auto p-6 bg-gradient-to-r from-amber-500/10 to-orange-500/10 border border-amber-500/20 rounded-2xl">
        <div className="flex items-center gap-3">
          <div className="p-3 bg-amber-500/20 border border-amber-500/30 rounded-lg">
            <Database className="w-6 h-6 text-amber-400" />
          </div>
          <div>
            <h2 className="text-xl font-bold text-white">
              Library Initializing
            </h2>
            <p className="text-gray-300">
              Please wait while the library is being set up...
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto">
      {/* Overall Statistics Header */}
      <div className="p-6 bg-white/5 backdrop-blur-sm border border-white/10 rounded-2xl shadow-2xl mb-6">
        <div className="flex items-center gap-3 mb-6">
          <div className="p-3 bg-blue-500/20 border border-blue-500/30 rounded-lg">
            <BarChart3 className="w-6 h-6 text-blue-400" />
          </div>
          <div className="flex-1">
            <h2 className="text-xl font-bold text-white">
              Server Library Overview
            </h2>
            <p className="text-gray-300">
              {releasesWithMedia} of {totalReleases} releases have complete
              media files
            </p>
          </div>
          <div className="text-right">
            <div className="flex items-center gap-2 text-sm text-gray-400">
              <Clock className="w-4 h-4" />
              <span>Updated {formatLastUpdated(stats.lastUpdated)}</span>
            </div>
          </div>
        </div>

        {/* Overall Progress Bar */}
        {totalReleases > 0 && (
          <div className="mb-6">
            <div className="flex items-center justify-between text-sm mb-2">
              <span className="text-white font-medium">
                Overall Media Availability
              </span>
              <span className="text-gray-300">
                {releasesWithMedia} / {totalReleases} releases
              </span>
            </div>
            <div className="w-full bg-white/10 rounded-full h-3">
              <div
                className="h-3 rounded-full bg-gradient-to-r from-green-500 to-blue-500 transition-all duration-500"
                style={{ width: `${mediaProgress}%` }}
              />
            </div>
            <div className="flex items-center gap-2 mt-2">
              {mediaProgress === 100 ? (
                <CheckCircle2 className="w-4 h-4 text-green-400" />
              ) : (
                <AlertCircle className="w-4 h-4 text-amber-400" />
              )}
              <span className="text-sm text-gray-300">
                {mediaProgress === 100
                  ? "All releases have complete media files!"
                  : `${releasesWithoutMedia} releases need media files`}
              </span>
            </div>
          </div>
        )}

        {/* Release Type Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          {renderReleaseTypeCard(
            <Album className="w-5 h-5 text-blue-400" />,
            "Albums",
            albums.length,
            albumsWithMedia,
            albumProgress,
            "bg-blue-500",
          )}

          {renderReleaseTypeCard(
            <Disc3 className="w-5 h-5 text-purple-400" />,
            "EPs",
            eps.length,
            epsWithMedia,
            epProgress,
            "bg-purple-500",
          )}

          {renderReleaseTypeCard(
            <Music className="w-5 h-5 text-green-400" />,
            "Singles",
            singles.length,
            singlesWithMedia,
            singleProgress,
            "bg-green-500",
          )}
        </div>

        {/* Library Statistics Grid */}
        <StatusGrid columns={4}>
          <StatusCard label="Artists" value={totalArtists} icon={Users} />
          <StatusCard label="Releases" value={totalReleases} icon={Disc3} />
          <StatusCard label="Tracks" value={totalTracks} icon={Music} />
          <StatusCard
            label="Complete"
            value={`${Math.round(mediaProgress)}%`}
            icon={CheckCircle2}
          />
        </StatusGrid>
      </div>
    </div>
  );
};
