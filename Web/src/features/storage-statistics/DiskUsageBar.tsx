import * as React from "react";
import { formatBytes } from "@/features/storage-statistics/ByteFormatter.ts";

interface DiskUsageBarProps {
  totalBytes: number;
  freeBytes: number;
  libraryBytes: number;
}

export const DiskUsageBar: React.FC<DiskUsageBarProps> = ({
  totalBytes,
  freeBytes,
  libraryBytes,
}) => {
  const usedBytes = Math.max(0, totalBytes - freeBytes);
  const lib = Math.min(libraryBytes, usedBytes);
  const toPct = (n: number) =>
    totalBytes > 0 ? Math.max(0, Math.min(100, (n / totalBytes) * 100)) : 0;
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
        <LegendItem
          colorClass="bg-gradient-to-r from-emerald-500 to-blue-500"
          label={`Library ${formatBytes(libraryBytes)}`}
        />
        <LegendItem
          colorClass="bg-gray-600"
          label={`Other used ${formatBytes(Math.max(0, usedBytes - libraryBytes))}`}
        />
        <LegendItem
          colorClass="bg-white/20"
          label={`Free ${formatBytes(freeBytes)}`}
        />
      </div>
    </div>
  );
};

const LegendItem: React.FC<{ colorClass: string; label: string }> = ({
  colorClass,
  label,
}) => (
  <div className="flex items-center gap-2 text-gray-300">
    <span className={`inline-block w-3 h-3 rounded-sm ${colorClass}`} />
    <span className="text-xs">{label}</span>
  </div>
);
