import * as React from "react";
import { ProgressIndicator } from "@/components/progress/ProgressIndicator.tsx";

export interface DownloadStatusProps {}

export const DownloadStatus: React.FC<DownloadStatusProps> = () => {
  // Minimal mocked values until real fragment is wired up
  const release = undefined as unknown as
    | { title: string; artists: Array<{ name: string }>; year?: number }
    | undefined;
  const totalTracks = undefined as unknown as number | undefined;
  const downloadedTracks = 0;

  const progressPercent =
    totalTracks == null
      ? 0
      : Math.round((downloadedTracks / totalTracks) * 100);

  return (
    <div className="max-w-md mx-auto bg-zinc-900 text-white rounded-2xl shadow-lg p-6 space-y-4">
      {release && (
        <div className="flex flex-col space-y-1">
          <h2 className="text-xl font-bold truncate">{release.title}</h2>
          <p className="text-sm text-zinc-400">
            {release.artists.map((artist) => artist.name).join(", ")} &bull;{" "}
            {release.year} &bull; {totalTracks} tracks
          </p>
        </div>
      )}

      <ProgressIndicator progressPercent={progressPercent} />

      <p className="text-sm text-zinc-300">&nbsp;</p>
    </div>
  );
};
