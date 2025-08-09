import * as React from "react";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { FollowButton } from "@/components/buttons/FollowButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import { graphql } from "@/gql";
import { useMutation, useQuery } from "urql";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import {
  ContextMenu,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuLabel,
  ContextMenuSeparator,
  ContextMenuSub,
  ContextMenuSubContent,
  ContextMenuSubTrigger,
  ContextMenuTrigger,
} from "@/components/ui/context-menu.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";

export interface ArtistActionButtonsProps {
  artistId: string;
  isImporting: boolean;
}

const refreshArtistTopTracksMutation = graphql(`
  mutation RefreshArtistTopTracksFromHeader(
    $input: RefreshArtistTopTracksInput!
  ) {
    refreshArtistTopTracks(input: $input) {
      __typename
    }
  }
`);

const refreshArtistLastFmMutation = graphql(`
  mutation RefreshArtistLastFm($artistId: String!) {
    refreshArtistLastFm(input: { artistId: $artistId }) {
      __typename
    }
  }
`);

const reimportReleaseMutation = graphql(`
  mutation ReimportRelease($artistId: String!, $folder: String!) {
    reimportRelease(artistId: $artistId, releaseFolderName: $folder) {
      __typename
    }
  }
`);

const redownloadReleaseMutation = graphql(`
  mutation RedownloadRelease($artistId: String!, $folder: String!) {
    redownloadRelease(artistId: $artistId, releaseFolderName: $folder) {
      __typename
    }
  }
`);

export const ArtistActionButtons: React.FC<ArtistActionButtonsProps> = ({
  artistId,
  isImporting,
}) => {
  const [{ fetching: refreshingTop }, doRefreshTop] = useMutation(
    refreshArtistTopTracksMutation,
  );
  const [{ fetching: refreshingLastFm }, doRefreshLastFm] = useMutation(
    refreshArtistLastFmMutation,
  );
  const [{ fetching: reimporting }, doReimport] = useMutation(
    reimportReleaseMutation,
  );
  const [{ fetching: redownloading }, doRedownload] = useMutation(
    redownloadReleaseMutation,
  );
  const dispatch = useAppDispatch();

  const [{ data: topData }] = useQuery({
    query: graphql(`
      query ArtistTopTracksForQueue($artistId: ID!) {
        serverLibrary {
          artistById(id: $artistId) {
            id
            topTracks {
              title
              coverArtUrl
              track {
                trackNumber
                release {
                  folderName
                  artist {
                    id
                    name
                  }
                }
              }
            }
          }
        }
      }
    `),
    variables: { artistId },
    pause: !artistId,
  });

  const [{ data: releasesData }] = useQuery({
    query: graphql(`
      query ArtistReleasesForContext($artistId: ID!) {
        serverLibrary {
          artistById(id: $artistId) {
            id
            releases {
              folderName
              title
            }
          }
        }
      }
    `),
    variables: { artistId },
    pause: !artistId,
  });

  const onPlayTopTracks = () => {
    const tts = topData?.serverLibrary.artistById?.topTracks ?? [];
    const queue = tts
      .filter((t) => t.track)
      .map((t) => ({
        artistId: t.track!.release.artist.id,
        releaseFolderName: t.track!.release.folderName,
        trackNumber: t.track!.trackNumber,
        title: t.title,
        artistName: t.track!.release.artist.name,
        coverArtUrl: t.coverArtUrl ?? undefined,
      }));
    if (queue.length > 0) {
      dispatch(musicPlayerSlice.actions.enqueueAndPlay(queue));
    }
  };

  const anyLoading =
    refreshingTop ||
    refreshingLastFm ||
    reimporting ||
    redownloading ||
    isImporting;

  const releases =
    releasesData?.serverLibrary.artistById?.releases?.map((r) => ({
      folderName: r.folderName,
      title: r.title,
    })) ?? [];

  return (
    <div className="px-6 md:px-10 py-6 flex items-center gap-4">
      <LargePlayButton onClick={onPlayTopTracks} />
      <ShuffleButton />
      <FollowButton />
      <ContextMenu>
        <ContextMenuTrigger asChild>
          <DotsButton />
        </ContextMenuTrigger>
        <ContextMenuContent>
          <ContextMenuLabel>Refresh</ContextMenuLabel>
          <ContextMenuItem
            onSelect={() => doRefreshTop({ input: { artistId } })}
          >
            Refresh top tracks
          </ContextMenuItem>
          <ContextMenuItem onSelect={() => doRefreshLastFm({ artistId })}>
            Refresh artist (Last.fm)
          </ContextMenuItem>
          <ContextMenuSeparator />
          <ContextMenuSub>
            <ContextMenuSubTrigger>Reimport release…</ContextMenuSubTrigger>
            <ContextMenuSubContent>
              {releases.map((r) => (
                <ContextMenuItem
                  key={r.folderName}
                  onSelect={() =>
                    doReimport({ artistId, folder: r.folderName })
                  }
                >
                  {r.title}
                </ContextMenuItem>
              ))}
            </ContextMenuSubContent>
          </ContextMenuSub>
          <ContextMenuSub>
            <ContextMenuSubTrigger>Redownload release…</ContextMenuSubTrigger>
            <ContextMenuSubContent>
              {releases.map((r) => (
                <ContextMenuItem
                  key={r.folderName}
                  onSelect={() =>
                    doRedownload({ artistId, folder: r.folderName })
                  }
                >
                  {r.title}
                </ContextMenuItem>
              ))}
            </ContextMenuSubContent>
          </ContextMenuSub>
        </ContextMenuContent>
      </ContextMenu>
      {anyLoading && <Spinner size={"sm"}/>}
    </div>
  );
};
