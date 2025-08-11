import * as React from "react";
import { useState } from "react";
import { TrackCreditLinks } from "@/features/album/TrackCreditLinks.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { AlbumTrackTag } from "@/features/album/AlbumTrackTag.tsx";
import { Tag } from "@/components/text/Tag.tsx";
import {
  ContextMenuItem,
  ContextMenuSeparator,
} from "@/components/ui/context-menu.tsx";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { useMutation } from "urql";
import { PlaylistItemFixArtistMatchDialog } from "@/features/playlists/fix-artist-match/PlaylistItemFixArtistMatchDialog.tsx";
import { Link } from "react-router";

export interface PlaylistTrackItemProps {
  playlistId: string;
  playlistItem: FragmentType<typeof playlistTrackItemPlaylistItemFragment>;
  trackNumber: number;
}

const playlistTrackItemPlaylistItemFragment = graphql(`
  fragment PlaylistTrackItem_PlaylistItem on PlaylistItem {
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

export const PlaylistTrackItem: React.FC<PlaylistTrackItemProps> = ({
  trackNumber,
  playlistId,
  ...props
}) => {
  const item = useFragment(
    playlistTrackItemPlaylistItemFragment,
    props.playlistItem,
  );

  const dispatch = useAppDispatch();

  const [, removeFromPlaylist] = useMutation(removeFromPlaylistMutation);
  const [, movePlaylistItem] = useMutation(movePlaylistItemMutation);

  const [fixOpenForItemId, setFixOpenForItemId] = useState<string | null>(null);

  const [dragIndex, setDragIndex] = useState<number | null>(null);

  return (
    <>
      <TrackItem
        key={item.id}
        title={item.title ?? ""}
        renderSubtitle={() =>
          item.track ? (
            <TrackCreditLinks track={item.track} />
          ) : item.artist ? (
            <Link to={`/artist/${item.artist.id}`} className="hover:underline">
              {item.artist.name}
            </Link>
          ) : (
            <span>{item.artistName}</span>
          )
        }
        trackNumber={trackNumber}
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
              await removeFromPlaylist({
                playlistId,
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
                qualityLabel: item.track.media?.audioQualityLabel ?? undefined,
              },
            ]),
          );
        }}
        draggable
        onDragStart={() => setDragIndex(trackNumber)}
        onDragOver={(ev) => ev.preventDefault()}
        onDrop={async () => {
          if (dragIndex == null || dragIndex === trackNumber) return;
          await movePlaylistItem({
            newIndex: dragIndex,
            playlistId,
            playlistItemId: item.id,
          });
          setDragIndex(null);
        }}
      />
      {fixOpenForItemId && (
        <PlaylistItemFixArtistMatchDialog
          open={!!fixOpenForItemId}
          onOpenChange={(open) => !open && setFixOpenForItemId(null)}
          playlistId={playlistId}
          playlistItemId={fixOpenForItemId ?? ""}
          initialArtistQuery={item.artistName}
          initialTrackQuery={item.title}
        />
      )}
    </>
  );
};
