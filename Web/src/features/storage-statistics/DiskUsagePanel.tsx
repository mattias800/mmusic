import * as React from "react";
import { GlassCard, StatusCard, StatusGrid } from "@/components/ui";
import {
  AlertTriangle,
  CheckCircle,
  HardDrive,
  TrendingUp,
} from "lucide-react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { DiskUsageBar } from "@/features/storage-statistics/DiskUsageBar.tsx";
import { formatBytes } from "@/features/storage-statistics/ByteFormatter.ts";

export interface DiskUsagePanelProps {
  storageStats: FragmentType<typeof diskUsagePanelStorageStatsFragment>;
}

const diskUsagePanelStorageStatsFragment = graphql(`
  fragment DiskUsagePanel_StorageStats on StorageStats {
    availableFreeBytes
    estimatedTotalLibrarySizeBytes
    librarySizeBytes
    totalDiskBytes
  }
`);

export const DiskUsagePanel: React.FC<DiskUsagePanelProps> = (props) => {
  const {
    availableFreeBytes,
    librarySizeBytes,
    totalDiskBytes,
    estimatedTotalLibrarySizeBytes,
  } = useFragment(diskUsagePanelStorageStatsFragment, props.storageStats);

  return (
    <GlassCard
      title="Storage Overview"
      icon={HardDrive}
      iconBgColor="bg-blue-500/20"
    >
      <div className="space-y-6">
        {/* Storage Overview */}
        <div>
          <StatusGrid columns={2}>
            <StatusCard
              label="Total Disk Space"
              value={totalDiskBytes ? formatBytes(totalDiskBytes) : "—"}
            />
            <StatusCard
              label="Free Space"
              value={
                availableFreeBytes
                  ? formatBytes(availableFreeBytes)
                  : "Not available"
              }
            />
            <StatusCard
              label="Current Library"
              value={formatBytes(librarySizeBytes)}
            />
            <StatusCard
              label="Estimated Total"
              value={formatBytes(estimatedTotalLibrarySizeBytes)}
            />
          </StatusGrid>
        </div>

        {/* Disk Usage Visualization */}
        {totalDiskBytes && availableFreeBytes ? (
          <div>
            <h3 className="text-lg font-semibold text-white mb-4 flex items-center gap-2">
              <TrendingUp className="w-5 h-5 text-green-400" />
              Disk Usage
            </h3>
            <DiskUsageBar
              totalBytes={totalDiskBytes}
              freeBytes={availableFreeBytes}
              libraryBytes={librarySizeBytes}
            />
          </div>
        ) : (
          <div>
            <h3 className="text-lg font-semibold text-white mb-4 flex items-center gap-2">
              <AlertTriangle className="w-5 h-5 text-yellow-400" />
              Disk Usage Information
            </h3>
            <div className="text-sm text-gray-400">
              Free space information is not available. Only showing library
              size.
            </div>
          </div>
        )}

        {/* Storage Warning */}
        {totalDiskBytes &&
          availableFreeBytes &&
          estimatedTotalLibrarySizeBytes > librarySizeBytes && (
            <StorageWarning
              totalBytes={totalDiskBytes}
              freeBytes={availableFreeBytes}
              currentLibraryBytes={librarySizeBytes}
              estimatedTotalLibraryBytes={estimatedTotalLibrarySizeBytes}
            />
          )}
      </div>
    </GlassCard>
  );
};

const StorageWarning: React.FC<{
  totalBytes: number;
  freeBytes: number;
  currentLibraryBytes: number;
  estimatedTotalLibraryBytes: number;
}> = ({
  totalBytes,
  freeBytes,
  currentLibraryBytes,
  estimatedTotalLibraryBytes,
}) => {
  const additionalSpaceNeeded =
    estimatedTotalLibraryBytes - currentLibraryBytes;
  const hasEnoughSpace = freeBytes >= additionalSpaceNeeded;

  if (hasEnoughSpace) {
    return (
      <div className="p-4 bg-gradient-to-r from-emerald-500/10 to-blue-500/10 border border-emerald-500/20 rounded-xl">
        <div className="flex items-start gap-3">
          <div className="flex-shrink-0">
            <div className="p-2 bg-emerald-500/20 rounded-lg">
              <CheckCircle className="w-5 h-5 text-emerald-400" />
            </div>
          </div>
          <div className="flex-1 min-w-0">
            <h3 className="text-sm font-medium text-emerald-200">
              Storage Planning Complete
            </h3>
            <p className="mt-1 text-sm text-emerald-300">
              You have sufficient free space ({formatBytes(freeBytes)}) to
              accommodate the estimated library growth of{" "}
              {formatBytes(additionalSpaceNeeded)}.
            </p>
          </div>
        </div>
      </div>
    );
  }

  const spaceShortfall = additionalSpaceNeeded - freeBytes;
  const shortfallPercentage = Math.round((spaceShortfall / totalBytes) * 100);

  return (
    <div className="p-4 bg-gradient-to-r from-amber-500/10 to-orange-500/10 border border-amber-500/20 rounded-xl">
      <div className="flex items-start gap-3">
        <div className="flex-shrink-0">
          <div className="p-2 bg-amber-500/20 rounded-lg">
            <AlertTriangle className="w-5 h-5 text-amber-400" />
          </div>
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-sm font-medium text-amber-200">
            Storage Planning Required
          </h3>
          <p className="mt-1 text-sm text-amber-300">
            To fully populate your library, you'll need an additional{" "}
            <span className="font-semibold">{formatBytes(spaceShortfall)}</span>{" "}
            of disk space.
          </p>
          <div className="mt-3 space-y-2">
            <div className="flex items-center justify-between text-xs">
              <span className="text-amber-400">Current free space:</span>
              <span className="text-amber-300">{formatBytes(freeBytes)}</span>
            </div>
            <div className="flex items-center justify-between text-xs">
              <span className="text-amber-400">Additional space needed:</span>
              <span className="text-amber-300">
                {formatBytes(additionalSpaceNeeded)}
              </span>
            </div>
            <div className="flex items-center justify-between text-xs">
              <span className="text-amber-400">Shortfall:</span>
              <span className="text-amber-300 font-semibold">
                {formatBytes(spaceShortfall)} ({shortfallPercentage}% of total
                disk)
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
