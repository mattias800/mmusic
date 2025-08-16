import * as React from "react";
import { StatusCard, StatusGrid } from "@/components/ui";
import { HardDrive, TrendingUp, AlertTriangle, CheckCircle } from "lucide-react";

export interface DiskUsagePanelProps {
  totalBytes?: number;
  freeBytes?: number;
  libraryBytes: number;
  estimatedTotalLibraryBytes: number;
}

export const DiskUsagePanel: React.FC<DiskUsagePanelProps> = ({ totalBytes, freeBytes, libraryBytes, estimatedTotalLibraryBytes }) => {
  return (
    <div className="space-y-6">
      {/* Storage Overview */}
      <div>
        <h3 className="text-lg font-semibold text-white mb-4 flex items-center gap-2">
          <HardDrive className="w-5 h-5 text-blue-400" />
          Storage Overview
        </h3>
        <StatusGrid columns={2}>
          <StatusCard 
            label="Total Disk Space" 
            value={totalBytes ? formatBytes(totalBytes) : "—"} 
          />
          <StatusCard 
            label="Free Space" 
            value={freeBytes ? formatBytes(freeBytes) : "Not available"} 
          />
          <StatusCard 
            label="Current Library" 
            value={formatBytes(libraryBytes)} 
          />
          <StatusCard 
            label="Estimated Total" 
            value={formatBytes(estimatedTotalLibraryBytes)} 
          />
        </StatusGrid>
      </div>

      {/* Disk Usage Visualization */}
      {totalBytes && freeBytes ? (
        <div>
          <h3 className="text-lg font-semibold text-white mb-4 flex items-center gap-2">
            <TrendingUp className="w-5 h-5 text-green-400" />
            Disk Usage
          </h3>
          <DiskUsageBar
            totalBytes={totalBytes}
            freeBytes={freeBytes}
            libraryBytes={libraryBytes}
          />
        </div>
      ) : (
        <div>
          <h3 className="text-lg font-semibold text-white mb-4 flex items-center gap-2">
            <AlertTriangle className="w-5 h-5 text-yellow-400" />
            Disk Usage Information
          </h3>
          <div className="text-sm text-gray-400">
            Free space information is not available. Only showing library size.
          </div>
        </div>
      )}
      
      {/* Storage Warning */}
      {totalBytes && freeBytes && estimatedTotalLibraryBytes > libraryBytes && (
        <StorageWarning 
          totalBytes={totalBytes}
          freeBytes={freeBytes}
          currentLibraryBytes={libraryBytes}
          estimatedTotalLibraryBytes={estimatedTotalLibraryBytes}
        />
      )}
    </div>
  );
};

const DiskUsageBar: React.FC<{
  totalBytes: number;
  freeBytes: number;
  libraryBytes: number;
}> = ({ totalBytes, freeBytes, libraryBytes }) => {
  const usedBytes = Math.max(0, totalBytes - freeBytes);
  const lib = Math.min(libraryBytes, usedBytes);
  const toPct = (n: number) => (totalBytes > 0 ? Math.max(0, Math.min(100, (n / totalBytes) * 100)) : 0);
  const libPct = toPct(lib);
  const otherUsedPct = toPct(usedBytes - lib);
  const freePct = toPct(freeBytes);

  return (
    <div className="w-full">
      <div className="w-full h-4 rounded-lg overflow-hidden bg-white/5 ring-1 ring-white/10 flex shadow-inner">
        {libPct > 0 && (
          <div
            className="h-full bg-gradient-to-r from-emerald-500 to-blue-500 transition-all duration-300"
            style={{ width: `${libPct}%` }}
            title={`Library: ${formatBytes(libraryBytes)}`}
          />
        )}
        {otherUsedPct > 0 && (
          <div
            className="h-full bg-gray-600 transition-all duration-300"
            style={{ width: `${otherUsedPct}%` }}
            title={`Other used: ${formatBytes(Math.max(0, usedBytes - libraryBytes))}`}
          />
        )}
        {freePct > 0 && (
          <div
            className="h-full bg-white/20 transition-all duration-300"
            style={{ width: `${freePct}%` }}
            title={`Free: ${formatBytes(freeBytes)}`}
          />
        )}
      </div>
      <div className="mt-3 grid grid-cols-3 gap-3 text-sm">
        <LegendItem colorClass="bg-gradient-to-r from-emerald-500 to-blue-500" label={`Library ${formatBytes(libraryBytes)}`} />
        <LegendItem colorClass="bg-gray-600" label={`Other used ${formatBytes(Math.max(0, usedBytes - libraryBytes))}`} />
        <LegendItem colorClass="bg-white/20" label={`Free ${formatBytes(freeBytes)}`} />
      </div>
    </div>
  );
};

const LegendItem: React.FC<{ colorClass: string; label: string }> = ({ colorClass, label }) => (
  <div className="flex items-center gap-2 text-gray-300">
    <span className={`inline-block w-3 h-3 rounded-sm ${colorClass}`} />
    <span className="text-xs">{label}</span>
  </div>
);

const StorageWarning: React.FC<{
  totalBytes: number;
  freeBytes: number;
  currentLibraryBytes: number;
  estimatedTotalLibraryBytes: number;
}> = ({ totalBytes, freeBytes, currentLibraryBytes, estimatedTotalLibraryBytes }) => {
  const additionalSpaceNeeded = estimatedTotalLibraryBytes - currentLibraryBytes;
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
            <h3 className="text-sm font-medium text-emerald-200">Storage Planning Complete</h3>
            <p className="mt-1 text-sm text-emerald-300">
              You have sufficient free space ({formatBytes(freeBytes)}) to accommodate the estimated library growth of {formatBytes(additionalSpaceNeeded)}.
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
          <h3 className="text-sm font-medium text-amber-200">Storage Planning Required</h3>
          <p className="mt-1 text-sm text-amber-300">
            To fully populate your library, you'll need an additional <span className="font-semibold">{formatBytes(spaceShortfall)}</span> of disk space.
          </p>
          <div className="mt-3 space-y-2">
            <div className="flex items-center justify-between text-xs">
              <span className="text-amber-400">Current free space:</span>
              <span className="text-amber-300">{formatBytes(freeBytes)}</span>
            </div>
            <div className="flex items-center justify-between text-xs">
              <span className="text-amber-400">Additional space needed:</span>
              <span className="text-amber-300">{formatBytes(additionalSpaceNeeded)}</span>
            </div>
            <div className="flex items-center justify-between text-xs">
              <span className="text-amber-400">Shortfall:</span>
              <span className="text-amber-300 font-semibold">{formatBytes(spaceShortfall)} ({shortfallPercentage}% of total disk)</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

function formatBytes(bytes: number): string {
  const units = ["B", "KB", "MB", "GB", "TB", "PB"] as const;
  let i = 0;
  let n = bytes;
  while (n >= 1024 && i < units.length - 1) {
    n /= 1024;
    i++;
  }
  const formatted = n >= 100 ? Math.round(n).toString() : n.toFixed(1);
  return `${formatted} ${units[i]}`;
}


