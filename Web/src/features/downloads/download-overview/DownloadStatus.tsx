import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";

export interface DownloadStatusProps {
  downloadStatus: FragmentType<typeof downloadStatusDownloadStatusFragment>;
}

export const downloadStatusDownloadStatusFragment = graphql(`
  fragment DownloadStatus_DownloadStatus on DownloadStatus {
    id
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
  const data = useFragment(
    downloadStatusDownloadStatusFragment,
    props.downloadStatus,
  );

  const release = data.release;
  const totalTracks = release?.recordings.length;
  const downloadedTracks = 3; // You could replace this with real-time progress from props/state

  const progressPercent =
    totalTracks == null
      ? 0
      : Math.round((downloadedTracks / totalTracks) * 100);

  return (
    <div className="max-w-md mx-auto bg-zinc-900 text-white rounded-2xl shadow-lg p-6 space-y-4">
      <div className="flex flex-col space-y-1">
        <h2 className="text-xl font-bold truncate">{release?.title}</h2>
        <p className="text-sm text-zinc-400">
          {release?.artists.map((artist) => artist.name).join(", ")} &bull;{" "}
          {release?.year}
        </p>
      </div>

      <div className="w-full bg-zinc-700 rounded-full h-2.5 overflow-hidden">
        <div
          className="bg-green-500 h-full transition-all duration-300"
          style={{ width: `${progressPercent}%` }}
        ></div>
      </div>

      <p className="text-sm text-zinc-300">
        Downloading {downloadedTracks} of {totalTracks} tracks
      </p>
    </div>
  );
};
