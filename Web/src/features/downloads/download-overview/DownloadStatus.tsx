import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";

export interface DownloadStatusProps {
  downloadStatus: FragmentType<typeof downloadStatusDownloadStatusFragment>;
}

export const downloadStatusDownloadStatusFragment = graphql(`
  fragment DownloadStatus_DownloadStatus on DownloadStatus {
    id
    numberOfTracks
    tracksDownloaded
    statusDescription
    release {
      id
      title
      year
      artists {
        id
        name
      }
      recordings {
        id
      }
    }
  }
`);

export const DownloadStatus: React.FC<DownloadStatusProps> = (props) => {
  const downloadStatus = useFragment(
    downloadStatusDownloadStatusFragment,
    props.downloadStatus,
  );

  const release = downloadStatus.release;
  const totalTracks = downloadStatus.numberOfTracks;
  const downloadedTracks = downloadStatus.tracksDownloaded ?? 0;

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

      <div className="w-full bg-zinc-700 rounded-full h-2.5 overflow-hidden">
        <div
          className="bg-green-500 h-full transition-all duration-300"
          style={{ width: `${progressPercent}%` }}
        ></div>
      </div>

      <p className="text-sm text-zinc-300">
        {downloadStatus?.statusDescription}
      </p>
    </div>
  );
};
