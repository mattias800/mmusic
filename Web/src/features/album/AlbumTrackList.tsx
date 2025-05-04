import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";

interface AlbumTrackListProps {
  release: FragmentType<typeof albumTrackListReleaseFragment>;
}

export const albumTrackListReleaseFragment = graphql(`
  fragment AlbumTrackList_Release on Release {
    id
    recordings {
      id
      title
      length
      artists {
        id
        name
      }
    }
  }
`);

export const AlbumTrackList: React.FC<AlbumTrackListProps> = (props) => {
  const release = useFragment(albumTrackListReleaseFragment, props.release);

  return (
    <div>
      {/* Song List Header */}
      <div className="grid grid-cols-[60px_1fr_auto_auto] md:grid-cols-[60px_1fr_auto_auto] items-center text-sm text-gray-400 border-b border-white/20 pb-2 mb-2 px-2">
        <span className="text-center text-white/60">#</span>
        <span className="text-left">Title</span>
        <span className="hidden md:block text-right pr-8">Plays</span>
        <span className="hidden md:block text-right pr-4">⏱️</span>
      </div>
      {/* Songs */}
      {[
        ["Club Bizarre", "31,526,462", "5:00"],
        ["Jack", "161,328", "4:05"],
        ["Love Religion - Video Edit", "5,260,253", "3:34"],
        ["Die Mission", "166,222", "4:15"],
        ["Movin'", "173,875", "5:28"],
        ["If Looks Could Kill", "134,284", "6:20"],
        ["Joy", "89,328", "4:55"],
        ["Das Boot II", "937,166", "5:17"],
        ["Dark Room Rituals", "82,546", "4:57"],
      ].map(([title, plays, duration], idx) => (
        <div
          key={title}
          className="grid grid-cols-[60px_1fr_auto_auto] md:grid-cols-[60px_1fr_auto_auto] items-center text-sm hover:bg-white/10 py-2 px-2 rounded"
        >
          <span className="text-center text-white/60">{idx + 1}</span>
          <div className="text-left">
            <div className="font-medium">{title}</div>
            <div className="text-white/50 text-xs">U96</div>
          </div>
          <span className="hidden md:block text-right pr-8 text-white/70">
            {plays}
          </span>
          <span className="hidden md:block text-right pr-4 text-white/70">
            {duration}
          </span>
        </div>
      ))}
    </div>
  );
};
