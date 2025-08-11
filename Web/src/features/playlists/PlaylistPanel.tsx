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
import { TrackCreditLinks } from "@/features/album/TrackCreditLinks.tsx";
import { PlaylistHeader } from "@/features/playlists/PlaylistHeader.tsx";
import { Tag } from "@/components/text/Tag.tsx";
import { TrackListHeading } from "@/components/track-item/TrackListHeading.tsx";
import { Button } from "@/components/ui/button.tsx";
import { Users } from "lucide-react";
import { graphql as g } from "@/gql";
import { PlaylistItemFixArtistMatchDialog } from "@/features/playlists/fix-artist-match/PlaylistItemFixArtistMatchDialog.tsx";

export interface PlaylistPanelProps {
  playlist: FragmentType<typeof playlistPanelPlaylistFragment>;
}

const playlistPanelPlaylistFragment = graphql(`
  fragment PlaylistPanel_Playlist on Playlist {
    id
    ...PlaylistHeader_Playlist
    name
    items {
      id
      title
      artistName
      coverImageUrl
      trackLengthMs
      artist {
        id
        name
      }
      track {
        ...TrackCreditLinks_Track
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
  mutation RemoveItemFromPlaylist($playlistId: ID!, $playlistItemId: ID!) {
    removeItemFromPlaylist(
      input: { playlistId: $playlistId, playlistItemId: $playlistItemId }
    ) {
      __typename
      ... on RemoveItemFromPlaylistSuccess {
        playlist {
          id
        }
      }
      ... on RemoveItemFromPlaylistError {
        message
      }
    }
  }
`);

const movePlaylistItemMutation = graphql(`
  mutation MovePlaylistItem(
    $newIndex: Int!
    $playlistId: ID!
    $playlistItemId: ID!
  ) {
    movePlaylistItem(
      input: {
        newIndex: $newIndex
        playlistId: $playlistId
        playlistItemId: $playlistItemId
      }
    ) {
      __typename
      ... on MovePlaylistItemSuccess {
        playlist {
          id
        }
      }
      ... on MovePlaylistItemError {
        message
      }
    }
  }
`);

const enqueueMissingArtistsMutation = g(`
  mutation EnqueueMissingArtistsFromPlaylist($playlistId: ID!) {
    enqueueMissingArtistsFromPlaylist(playlistId: $playlistId)
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
  const [, enqueueMissing] = useMutation(enqueueMissingArtistsMutation);

  const enqueueAllMissing = async () => {
    await enqueueMissing({ playlistId: playlist.id });
  };

  const [dragIndex, setDragIndex] = React.useState<number | null>(null);
  const [fixOpenForItemId, setFixOpenForItemId] = React.useState<string | null>(null);

  return (
    <GradientContent>
      <MainPadding>
        <PlaylistHeader playlist={playlist} />
        <div className="mt-4 p-4 rounded-md border bg-muted/30 flex items-center justify-between">
          <div>
            <div className="font-medium">Missing artists detected</div>
            <div className="text-sm text-zinc-400">Queue all missing artists from this playlist for import.</div>
          </div>
          <Button size="sm" iconLeft={Users} onClick={enqueueAllMissing}>
            Enqueue missing artists
          </Button>
        </div>
        <div className="mb-12" />

        <TrackListHeading showCoverArt />

        {items.map((item, index) => (
          <TrackItem
            key={item.id}
            title={item.title ?? ""}
            renderSubtitle={() =>
              item.track ? (
                <TrackCreditLinks track={item.track} />
              ) : (
                <span>{item.artistName}</span>
              )
            }
            trackNumber={index + 1}
            playCount={0}
            trackLength={item.trackLengthMs}
            showCoverArt
            renderCoverArt={() => (
              <ReleaseCoverArt
                className="h-12 w-12"
                srcUrl={item.coverImageUrl ?? item.track?.release?.coverArtUrl}
                artistThumbUrl={item.track?.release.artist.images?.thumbs?.[0]}
                titleForPlaceholder={
                  item.track?.release.artist.name ?? item.artistName ?? ""
                }
                alt={item.title ?? item.track?.title ?? ""}
              />
            )}
            renderTag={() =>
              item.track ? (
                <AlbumTrackTag track={item.track} />
              ) : (
                <div className={"flex gap-2 items-center"}>
                  <Tag variant={"error"}>Missing track</Tag>
                  {item.artist == null ? (
                    <>
                      <Tag variant={"error"}>Missing artist</Tag>
                      <button
                        className="text-xs px-2 py-1 rounded bg-secondary text-secondary-foreground"
                        onClick={() => setFixOpenForItemId(item.id)}
                      >
                        Fix match
                      </button>
                    </>
                  ) : null}
                </div>
              )
            }
            contextMenuItems={[
              <ContextMenuItem
                key="remove"
                onClick={async () => {
                  setItems((prev) => prev.filter((t) => t.id !== item.id));
                  await removeFromPlaylist({
                    playlistId: playlist.id,
                    playlistItemId: item.id,
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
                  newIndex: index,
                  playlistId: playlist.id,
                  playlistItemId: item.id,
                });
              }
              setDragIndex(null);
            }}
          />
        ))}
        <PlaylistItemFixArtistMatchDialog
          open={!!fixOpenForItemId}
          onOpenChange={(open) => !open && setFixOpenForItemId(null)}
          playlistId={playlist.id}
          playlistItemId={fixOpenForItemId ?? ""}
          initialArtistQuery={items.find(i => i.id === fixOpenForItemId)?.artistName ?? undefined}
          initialTrackQuery={items.find(i => i.id === fixOpenForItemId)?.title ?? undefined}
        />
      </MainPadding>
    </GradientContent>
  );
};
