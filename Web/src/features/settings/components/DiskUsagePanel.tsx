import * as React from "react";

export interface DiskUsagePanelProps {
  totalBytes?: number;
  freeBytes?: number;
  libraryBytes: number;
  estimatedTotalLibraryBytes: number;
}

export const DiskUsagePanel: React.FC<DiskUsagePanelProps> = ({ totalBytes, freeBytes, libraryBytes, estimatedTotalLibraryBytes }) => {
  return (
    <div className="flex flex-col gap-3">
      <div className="max-w-2xl">
        <DiskUsageBar
          totalBytes={totalBytes ?? 0}
          freeBytes={freeBytes ?? 0}
          libraryBytes={libraryBytes}
        />
      </div>
      <div className="max-w-2xl">
        <StatsBox 
          totalBytes={totalBytes} 
          freeBytes={freeBytes} 
          libraryBytes={libraryBytes} 
          estimatedTotalLibraryBytes={estimatedTotalLibraryBytes}
        />
      </div>
      
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
      <div className="w-full h-4 rounded-md overflow-hidden bg-zinc-900 ring-1 ring-zinc-700 flex">
        {libPct > 0 && (
          <div
            className="h-full bg-gradient-to-r from-emerald-500 to-blue-500"
            style={{ width: `${libPct}%` }}
            title={`Library: ${formatBytes(libraryBytes)}`}
          />
        )}
        {otherUsedPct > 0 && (
          <div
            className="h-full bg-zinc-600"
            style={{ width: `${otherUsedPct}%` }}
            title={`Other used: ${formatBytes(Math.max(0, usedBytes - libraryBytes))}`}
          />
        )}
        {freePct > 0 && (
          <div
            className="h-full bg-zinc-800"
            style={{ width: `${freePct}%` }}
            title={`Free: ${formatBytes(freeBytes)}`}
          />
        )}
      </div>
      <div className="mt-2 grid grid-cols-3 gap-2 text-sm">
        <LegendItem colorClass="bg-gradient-to-r from-emerald-500 to-blue-500" label={`Library ${formatBytes(libraryBytes)}`} />
        <LegendItem colorClass="bg-zinc-600" label={`Other used ${formatBytes(Math.max(0, usedBytes - libraryBytes))}`} />
        <LegendItem colorClass="bg-zinc-800" label={`Free ${formatBytes(freeBytes)}`} />
      </div>
    </div>
  );
};

const LegendItem: React.FC<{ colorClass: string; label: string }> = ({ colorClass, label }) => (
  <div className="flex items-center gap-2 text-zinc-300">
    <span className={`inline-block w-3 h-3 rounded-sm ${colorClass}`} />
    <span>{label}</span>
  </div>
);

const StatsBox: React.FC<{
  totalBytes?: number;
  freeBytes?: number;
  libraryBytes: number;
  estimatedTotalLibraryBytes: number;
}> = ({ totalBytes, freeBytes, libraryBytes, estimatedTotalLibraryBytes }) => {
  // Calculate growth factor and percentage
  const growthFactor = libraryBytes > 0 ? estimatedTotalLibraryBytes / libraryBytes : 1;
  const growthPercentage = Math.round((growthFactor - 1) * 100);
  
  return (
    <div className="rounded-md border border-zinc-800 bg-zinc-900 p-3 text-sm text-zinc-300">
      <div className="grid grid-cols-1 sm:grid-cols-4 gap-3">
        <div className="flex flex-col">
          <span className="text-xs text-zinc-400">Total disk</span>
          <span className="text-base font-medium">{totalBytes == null ? "—" : formatBytes(totalBytes)}</span>
        </div>
        <div className="flex flex-col">
          <span className="text-xs text-zinc-400">Free disk</span>
          <span className="text-base font-medium">{freeBytes == null ? "—" : formatBytes(freeBytes)}</span>
        </div>
        <div className="flex flex-col">
          <span className="text-xs text-zinc-400">Library size</span>
          <span className="text-base font-medium">{formatBytes(libraryBytes)}</span>
        </div>
        <div className="flex flex-col">
          <span className="text-xs text-zinc-400">Estimated total</span>
          <div className="flex items-center gap-2">
            <span className="text-base font-medium">{formatBytes(estimatedTotalLibraryBytes)}</span>
            {growthFactor > 1 && (
              <span className="text-xs px-2 py-1 bg-blue-600/20 text-blue-400 rounded">
                +{growthPercentage}%
              </span>
            )}
          </div>
        </div>
      </div>
      
      {/* Growth indicator */}
      {growthFactor > 1 && (
        <div className="mt-3 pt-3 border-t border-zinc-700">
          <div className="flex items-center justify-between text-xs">
            <span className="text-zinc-400">Library growth potential</span>
            <span className="text-zinc-300">
              {formatBytes(estimatedTotalLibraryBytes - libraryBytes)} additional space needed
            </span>
          </div>
          <div className="mt-2 w-full bg-zinc-700 rounded-full h-2">
            <div 
              className="h-2 rounded-full bg-gradient-to-r from-emerald-500 to-blue-500 transition-all duration-300"
              style={{ width: `${Math.min(100, (libraryBytes / estimatedTotalLibraryBytes) * 100)}%` }}
            />
          </div>
        </div>
      )}
    </div>
  );
};

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
      <div className="max-w-2xl">
        <div className="rounded-md border border-emerald-700/50 bg-emerald-900/20 p-4">
          <div className="flex items-start gap-3">
            <div className="flex-shrink-0">
              <div className="p-2 bg-emerald-600/20 rounded-lg">
                <svg className="w-5 h-5 text-emerald-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
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
      </div>
    );
  }

  const spaceShortfall = additionalSpaceNeeded - freeBytes;
  const shortfallPercentage = Math.round((spaceShortfall / totalBytes) * 100);
  
  return (
    <div className="max-w-2xl">
      <div className="rounded-md border border-amber-700/50 bg-amber-900/20 p-4">
        <div className="flex items-start gap-3">
          <div className="flex-shrink-0">
            <div className="p-2 bg-amber-600/20 rounded-lg">
              <svg className="w-5 h-5 text-amber-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
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


