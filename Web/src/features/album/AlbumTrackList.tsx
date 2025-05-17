import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import {
  formatLargeNumber,
  formatTrackLength,
} from "@/common/TrackLengthFormatter.ts";
import { Link } from "react-router";
import { PlayButton } from "@/components/buttons/PlayButton.tsx";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { useAppDispatch } from "@/ReduxAppHooks.ts";

interface AlbumTrackListProps {
  releaseGroup: FragmentType<typeof albumTrackListReleaseGroupFragment>;
}

export const albumTrackListReleaseGroupFragment = graphql(`
  fragment AlbumTrackList_ReleaseGroup on ReleaseGroup {
    id
    title
    mainRelease {
      id
      title
      recordings {
        id
        title
        length
        youtubeMusicId
        statistics {
          playCount
        }
        nameCredits {
          name
          artist {
            id
            name
          }
        }
      }
    }
  }
`);

export const AlbumTrackList: React.FC<AlbumTrackListProps> = (props) => {
  const releaseGroup = useFragment(
    albumTrackListReleaseGroupFragment,
    props.releaseGroup,
  );

  const dispatch = useAppDispatch();

  const release = releaseGroup.mainRelease;

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
      {release?.recordings.map((recording, idx) => (
        <div
          key={recording.id}
          className="grid grid-cols-[60px_1fr_auto_auto] md:grid-cols-[60px_1fr_auto_auto] items-center text-sm hover:bg-white/10 py-2 px-2 rounded"
        >
          <span className="text-center text-white/60">{idx + 1}</span>
          <div className="text-left">
            <div className="font-medium">{recording.title}</div>
            <div className="text-white/50 text-xs">
              {recording.nameCredits.map(({ artist }, index) => (
                <React.Fragment key={artist.id}>
                  {index > 0 && ", "}
                  <Link to={`/artist/${artist.id}`} className="hover:underline">
                    {artist.name}
                  </Link>
                </React.Fragment>
              ))}
            </div>
          </div>
          <span className="hidden md:block text-right pr-8 ">
            {recording.youtubeMusicId && (
              <PlayButton
                onClick={() =>
                  recording?.youtubeMusicId &&
                  dispatch(
                    musicPlayerSlice.actions.openYoutubeMusicId({
                      youtubeId: recording.youtubeMusicId,
                    }),
                  )
                }
              />
            )}
            <span className={"text-white/70"}>
              {formatLargeNumber(recording.statistics?.playCount ?? 0)}
            </span>
          </span>
          <span className="hidden md:block text-right pr-4 text-white/70">
            {recording.length ? formatTrackLength(recording.length) : ""}
          </span>
        </div>
      ))}
    </div>
  );
};
