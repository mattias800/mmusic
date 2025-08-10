import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { GradientContent } from "@/components/page-body/GradientContent";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { AlbumTrackTag } from "@/features/album/AlbumTrackTag.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";

export interface PlaylistPanelProps {
  playlist: FragmentType<typeof playlistPanelPlaylistFragment>;
}

const playlistPanelPlaylistFragment = graphql(`
  fragment PlaylistPanel_Playlist on Playlist {
    id
    name
    tracks {
      id
      title
      trackLength
      trackNumber
      media {
        id
        audioQualityLabel
      }
      release {
        id
        folderName
        coverArtUrl
        artist {
          id
          name
          images {
            thumbs
          }
        }
      }
      ...AlbumTrackTag_Track
    }
  }
`);

export const PlaylistPanel: React.FC<PlaylistPanelProps> = (props) => {
  const playlist = useFragment(playlistPanelPlaylistFragment, props.playlist);
  const dispatch = useAppDispatch();

  return (
    <GradientContent>
      <MainPadding>
        <div className="flex flex-col gap-2">
          <h1 className="text-2xl font-semibold mb-4">
            {playlist.name ?? "Playlist"}
          </h1>

          {playlist.tracks.map((track) => (
            <TrackItem
              key={track.id}
              title={track.title}
              trackNumber={track.trackNumber}
              playCount={0}
              trackLength={track.trackLength}
              showCoverArt
              renderCoverArt={() => (
                <ReleaseCoverArt
                  className="h-12 w-12"
                  srcUrl={track.release.coverArtUrl}
                  artistThumbUrl={track.release.artist.images?.thumbs?.[0]}
                  titleForPlaceholder={track.release.artist.name}
                  alt={track.title}
                />
              )}
              renderTag={() => <AlbumTrackTag track={track} />}
              onClick={() => {
                dispatch(
                  musicPlayerSlice.actions.enqueueAndPlay([
                    {
                      artistId: track.release.artist.id,
                      releaseFolderName: track.release.folderName,
                      trackNumber: track.trackNumber,
                      title: track.title,
                      artistName: track.release.artist.name,
                      coverArtUrl: track.release.coverArtUrl,
                      trackLengthMs: track.trackLength ?? 0,
                      qualityLabel: track.media?.audioQualityLabel ?? undefined,
                    },
                  ]),
                );
              }}
            />
          ))}
        </div>
      </MainPadding>
    </GradientContent>
  );
};
