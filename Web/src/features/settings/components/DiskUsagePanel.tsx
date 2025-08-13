import * as React from "react";

export interface DiskUsagePanelProps {
  totalBytes?: number;
  freeBytes?: number;
  libraryBytes: number;
}

export const DiskUsagePanel: React.FC<DiskUsagePanelProps> = ({ totalBytes, freeBytes, libraryBytes }) => {
  return (
    <div className="flex flex-col gap-3">
      <div className="max-w-lg">
        <DiskUsageBar
          totalBytes={totalBytes ?? 0}
          freeBytes={freeBytes ?? 0}
          libraryBytes={libraryBytes}
        />
      </div>
      <div className="max-w-lg">
        <StatsBox totalBytes={totalBytes} freeBytes={freeBytes} libraryBytes={libraryBytes} />
      </div>
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
            className="h-full bg-emerald-400"
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
        <LegendItem colorClass="bg-emerald-400" label={`Library ${formatBytes(libraryBytes)}`} />
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
}> = ({ totalBytes, freeBytes, libraryBytes }) => {
  return (
    <div className="rounded-md border border-zinc-800 bg-zinc-900 p-3 text-sm text-zinc-300">
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
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


