import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { GradientContent } from "@/components/page-body/GradientContent";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { AlbumTrackTag } from "@/features/album/AlbumTrackTag.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { useMutation, useQuery } from "urql";
import { ContextMenuItem, ContextMenuSeparator } from "@/components/ui/context-menu.tsx";

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

  const [tracks, setTracks] = React.useState(() => playlist.tracks);
  React.useEffect(() => {
    setTracks(playlist.tracks);
  }, [playlist.id, playlist.tracks]);

  // Fetch viewer id for move mutation
  const viewerIdQuery = graphql(`
    query Bootstrap {
      areThereAnyUsers
      viewer { id }
    }
  `);
  const [{ data: viewerData }] = useQuery({ query: viewerIdQuery });

  // Mutations (cast to any to avoid codegen typings requirement during build)
  const [, removeFromPlaylist] = useMutation(`
    mutation RemoveTrackFromPlaylist(
      $playlistId: UUID!
      $artistId: String!
      $releaseFolderName: String!
      $trackNumber: Int!
    ) {
      removeTrackFromPlaylist(
        playlistId: $playlistId
        artistId: $artistId
        releaseFolderName: $releaseFolderName
        trackNumber: $trackNumber
      ) {
        __typename
        ... on RemoveTrackFromPlaylistSuccess { success }
        ... on RemoveTrackFromPlaylistError { message }
      }
    }
  `);
  const [, movePlaylistItem] = useMutation(`
    mutation MovePlaylistItem(
      $actorUserId: UUID!
      $artistId: String!
      $newIndex: Int!
      $playlistId: UUID!
      $releaseFolderName: String!
      $trackNumber: Int!
    ) {
      movePlaylistItem(
        actorUserId: $actorUserId
        artistId: $artistId
        newIndex: $newIndex
        playlistId: $playlistId
        releaseFolderName: $releaseFolderName
        trackNumber: $trackNumber
      ) {
        __typename
        ... on MovePlaylistItemSuccess { success }
        ... on MovePlaylistItemError { message }
      }
    }
  `);

  const [dragIndex, setDragIndex] = React.useState<number | null>(null);

  return (
    <GradientContent>
      <MainPadding>
        <div className="flex flex-col gap-2">
          <h1 className="text-2xl font-semibold mb-4">
            {playlist.name ?? "Playlist"}
          </h1>

          {tracks.map((track, index) => (
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
              contextMenuItems={[
                <ContextMenuItem
                  key="remove"
                  onClick={async () => {
                    setTracks((prev) => prev.filter((t) => t.id !== track.id));
                    await removeFromPlaylist({
                      playlistId: playlist.id,
                      artistId: track.release.artist.id,
                      releaseFolderName: track.release.folderName,
                      trackNumber: track.trackNumber,
                    });
                  }}
                >
                  Remove from this Playlist
                </ContextMenuItem>,
                <ContextMenuSeparator key="sep" />,
              ]}
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
              draggable
              onDragStart={() => setDragIndex(index)}
              onDragOver={(ev) => ev.preventDefault()}
              onDrop={async () => {
                if (dragIndex == null || dragIndex === index) return;
                setTracks((prev) => {
                  const next = prev.slice();
                  const [moved] = next.splice(dragIndex, 1);
                  next.splice(index, 0, moved);
                  return next;
                });
                if (viewerData?.viewer?.id) {
                  await movePlaylistItem({
                    actorUserId: viewerData.viewer.id,
                    artistId: track.release.artist.id,
                    newIndex: index,
                    playlistId: playlist.id,
                    releaseFolderName: track.release.folderName,
                    trackNumber: track.trackNumber,
                  });
                }
                setDragIndex(null);
              }}
            />
          ))}
        </div>
      </MainPadding>
    </GradientContent>
  );
};
