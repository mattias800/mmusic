import * as React from "react";
import { useEffect, useState } from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { GradientContent } from "@/components/page-body/GradientContent";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { AlbumTrackTag } from "@/features/album/AlbumTrackTag.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { useMutation, useQuery } from "urql";
import {
  ContextMenuItem,
  ContextMenuSeparator,
} from "@/components/ui/context-menu.tsx";

export interface PlaylistPanelProps {
  playlist: FragmentType<typeof playlistPanelPlaylistFragment>;
}

const playlistPanelPlaylistFragment = graphql(`
  fragment PlaylistPanel_Playlist on Playlist {
    id
    name
    items {
      id
      title
      artistName
      coverImageUrl
      trackLengthMs
      track {
        ...AlbumTrackTag_Track
        trackLength
        trackNumber
        media {
          id
          audioQualityLabel
        }
        title
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
      }
    }
  }
`);

const removeFromPlaylistMutation = graphql(`
  mutation RemoveTrackFromPlaylist(
    $playlistId: UUID!
    $artistId: String!
    $releaseFolderName: String!
    $trackNumber: Int!
  ) {
    removeTrackFromPlaylist(
      input: {
        playlistId: $playlistId
        artistId: $artistId
        releaseFolderName: $releaseFolderName
        trackNumber: $trackNumber
      }
    ) {
      __typename
      ... on RemoveTrackFromPlaylistSuccess { playlist { id } }
      ... on RemoveTrackFromPlaylistError { message }
    }
  }
`);

const movePlaylistItemMutation = graphql(`
  mutation MovePlaylistItem(
    $artistId: String!
    $newIndex: Int!
    $playlistId: UUID!
    $releaseFolderName: String!
    $trackNumber: Int!
  ) {
    movePlaylistItem(
      input: {
        artistId: $artistId
        newIndex: $newIndex
        playlistId: $playlistId
        releaseFolderName: $releaseFolderName
        trackNumber: $trackNumber
      }
    ) {
      __typename
      ... on MovePlaylistItemSuccess { playlist { id } }
      ... on MovePlaylistItemError { message }
    }
  }
`);

export const PlaylistPanel: React.FC<PlaylistPanelProps> = (props) => {
  const playlist = useFragment(playlistPanelPlaylistFragment, props.playlist);
  const dispatch = useAppDispatch();

  const [items, setItems] = useState(() => playlist.items);

  useEffect(() => {
    setItems(playlist.items);
  }, [playlist.id, playlist.items]);

  // Fetch viewer id for move mutation
  const viewerIdQuery = graphql(`
    query Bootstrap {
      areThereAnyUsers
      viewer {
        id
      }
    }
  `);
  const [{ data: viewerData }] = useQuery({ query: viewerIdQuery });

  const [, removeFromPlaylist] = useMutation(removeFromPlaylistMutation);
  const [, movePlaylistItem] = useMutation(movePlaylistItemMutation);

  const [dragIndex, setDragIndex] = React.useState<number | null>(null);

  const uniqueCoverUrls = React.useMemo(() => {
    const seen = new Set<string>();
    const urls: string[] = [];
    for (const t of playlist.items) {
      const url = t.coverImageUrl ?? "";
      if (url && !seen.has(url)) {
        seen.add(url);
        urls.push(url);
      }
    }
    return urls;
  }, [playlist.items]);

  const collageUrls = React.useMemo(
    () => uniqueCoverUrls.slice(0, 4),
    [uniqueCoverUrls],
  );

  return (
    <GradientContent>
      <MainPadding>
        <div className="flex flex-col gap-4">
          <div className="flex items-end gap-4 mb-2">
            <div>
              {collageUrls.length >= 4 ? (
                <div className="grid grid-cols-2 grid-rows-2 w-28 h-28 overflow-hidden rounded-md ring-1 ring-white/10">
                  {collageUrls.map((url, i) => (
                    <img
                      key={i}
                      src={url}
                      alt={`Cover ${i + 1}`}
                      className="w-full h-full object-cover"
                      draggable={false}
                    />
                  ))}
                </div>
              ) : (
                <div className="w-28 h-28">
                  {uniqueCoverUrls[0] ? (
                    <img
                      src={uniqueCoverUrls[0]}
                      alt="Playlist cover"
                      className="w-28 h-28 object-cover rounded-md ring-1 ring-white/10"
                      draggable={false}
                    />
                  ) : (
                    <div className="w-28 h-28 rounded-md bg-neutral-800 ring-1 ring-white/10" />
                  )}
                </div>
              )}
            </div>
            <div className="flex flex-col">
              <h1 className="text-2xl font-semibold">
                {playlist.name ?? "Playlist"}
              </h1>
              <div className="text-sm text-neutral-400">
                {items.length} {items.length === 1 ? "track" : "tracks"}
              </div>
            </div>
          </div>

          {items.map((item, index) => (
            <TrackItem
              key={item.id}
              title={item.title ?? ""}
              trackNumber={index}
              playCount={0}
              trackLength={item.trackLengthMs}
              showCoverArt
              renderCoverArt={() => (
                <ReleaseCoverArt
                  className="h-12 w-12"
                  srcUrl={
                    item.coverImageUrl ?? item.track?.release?.coverArtUrl
                  }
                  artistThumbUrl={
                    item.track?.release.artist.images?.thumbs?.[0]
                  }
                  titleForPlaceholder={
                    item.track?.release.artist.name ?? item.artistName ?? ""
                  }
                  alt={item.title ?? item.track?.title ?? ""}
                />
              )}
              renderTag={() => <AlbumTrackTag track={item.track} />}
              contextMenuItems={[
                <ContextMenuItem
                  key="remove"
                  onClick={async () => {
                    setItems((prev) => prev.filter((t) => t.id !== item.id));
                    await removeFromPlaylist({
                      playlistId: playlist.id,
                      artistId: item.track?.release.artist.id,
                      releaseFolderName: item.track?.release.folderName,
                      trackNumber: item.track?.trackNumber,
                    });
                  }}
                >
                  Remove from this Playlist
                </ContextMenuItem>,
                <ContextMenuSeparator key="sep" />,
              ]}
              onClick={() => {
                if (!item.track) {
                  return;
                }
                dispatch(
                  musicPlayerSlice.actions.enqueueAndPlay([
                    {
                      artistId: item.track.release.artist.id,
                      releaseFolderName: item.track.release.folderName,
                      trackNumber: item.track.trackNumber,
                      title: item.title ?? "",
                      artistName: item.track.release.artist.name,
                      coverArtUrl: item.track.release.coverArtUrl,
                      trackLengthMs: item.track.trackLength ?? 0,
                      qualityLabel:
                        item.track.media?.audioQualityLabel ?? undefined,
                    },
                  ]),
                );
              }}
              draggable
              onDragStart={() => setDragIndex(index)}
              onDragOver={(ev) => ev.preventDefault()}
              onDrop={async () => {
                if (dragIndex == null || dragIndex === index) return;
                setItems((prev) => {
                  const next = prev.slice();
                  const [moved] = next.splice(dragIndex, 1);
                  next.splice(index, 0, moved);
                  return next;
                });
                if (viewerData?.viewer?.id) {
                  await movePlaylistItem({
                    artistId: item.track?.release.artist.id,
                    newIndex: index,
                    playlistId: playlist.id,
                    releaseFolderName: item.track?.release.folderName,
                    trackNumber: item.track?.trackNumber,
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
