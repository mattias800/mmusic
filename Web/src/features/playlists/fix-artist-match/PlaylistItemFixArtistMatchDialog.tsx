import * as React from "react";
import { useCallback, useMemo, useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog.tsx";
import { Input } from "@/components/ui/input.tsx";
import { Button } from "@/components/ui/button.tsx";
import { graphql } from "@/gql";
import type { FixArtist_MbArtistSearchQuery } from "@/gql/graphql.ts";
import { useMutation, useQuery } from "urql";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";

const mbArtistSearchQuery = graphql(`
  query FixArtist_MbArtistSearch($artistName: String!, $trackName: String!) {
    musicBrainz {
      artist {
        searchByName(name: $artistName, limit: 15) {
          id
          name
          disambiguation
          country
          type
          listeners
          images {
            artistThumb
          }
          lastFmArtist {
            summary
          }
        }
      }
      recording {
        searchByName(name: $trackName, limit: 5) {
          id
          title
          artists {
            id
            name
          }
        }
      }
    }
  }
`);

const setPlaylistItemArtistMusicBrainzMatchMutation = graphql(`
  mutation SetPlaylistItemArtistMusicBrainzMatch(
    $input: SetPlaylistItemArtistMusicBrainzMatchInput!
  ) {
    setPlaylistItemArtistMusicBrainzMatch(input: $input) {
      __typename
      ... on SetPlaylistItemArtistMusicBrainzMatchSuccess {
        playlistItem {
          id
        }
      }
      ... on SetPlaylistItemArtistMusicBrainzMatchNotFound {
        message
      }
      ... on SetPlaylistItemArtistMusicBrainzMatchError {
        message
      }
    }
  }
`);

export interface PlaylistItemFixArtistMatchDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  playlistId: string;
  playlistItemId: string;
  initialArtistQuery?: string | undefined | null;
  initialTrackQuery?: string | undefined | null;
}

export const PlaylistItemFixArtistMatchDialog: React.FC<
  PlaylistItemFixArtistMatchDialogProps
> = ({
  initialArtistQuery,
  initialTrackQuery,
  onOpenChange,
  open,
  playlistId,
  playlistItemId,
}) => {
  const [artistQuery, setArtistQuery] = useState(initialArtistQuery ?? "");
  const [trackQuery, setTrackQuery] = useState(initialTrackQuery ?? "");

  const [variables, setVariables] = useState<{
    artistName: string;
    trackName: string;
  }>({
    artistName: initialArtistQuery ?? "",
    trackName: initialTrackQuery ?? "",
  });

  const [{ data, fetching }] = useQuery({
    query: mbArtistSearchQuery,
    variables,
    pause: !variables.artistName,
  });

  const [{ fetching: submitting }, setMbMatch] = useMutation(
    setPlaylistItemArtistMusicBrainzMatchMutation,
  );

  const updateVariables = useCallback(() => {
    setVariables({ trackName: trackQuery, artistName: artistQuery });
  }, [artistQuery, trackQuery]);

  // Initialize fields and optionally run a search when the dialog content mounts/opens
  const onDialogOpenAutoFocus = useCallback(() => {
    const initialArtist = (initialArtistQuery ?? "").trim();
    const initialTrack = (initialTrackQuery ?? "").trim();
    setArtistQuery(initialArtist);
    setTrackQuery(initialTrack);
    if (initialArtist.length > 0) {
      updateVariables();
    }
  }, [initialArtistQuery, initialTrackQuery, updateVariables]);

  // If track query present, bias artist list to those appearing on matching recordings
  const biasedArtistIds = useMemo(() => {
    const recs = (data?.musicBrainz?.recording?.searchByName ??
      []) as NonNullable<
      FixArtist_MbArtistSearchQuery["musicBrainz"]["recording"]["searchByName"]
    >;
    const ids = new Set<string>();
    recs.forEach((r) => r.artists?.forEach((a) => a?.id && ids.add(a.id)));
    return ids;
  }, [data]);

  const artists = useMemo(() => {
    const all = (data?.musicBrainz?.artist?.searchByName ?? []) as NonNullable<
      FixArtist_MbArtistSearchQuery["musicBrainz"]["artist"]["searchByName"]
    >;
    if (!biasedArtistIds.size) return all;
    return [...all].sort((a, b) => {
      const aBias = biasedArtistIds.has(a.id) ? 1 : 0;
      const bBias = biasedArtistIds.has(b.id) ? 1 : 0;
      return bBias - aBias;
    });
  }, [data, biasedArtistIds]);

  const chooseMb = async (musicBrainzArtistId: string) => {
    setSelectedId(musicBrainzArtistId);
    await setMbMatch({
      input: {
        playlistId: playlistId,
        playlistItemId: playlistItemId,
        musicBrainzArtistId,
      },
    });
    onOpenChange(false);
  };

  const [selectedId, setSelectedId] = useState<string | null>(null);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent onOpenAutoFocus={onDialogOpenAutoFocus}>
        <DialogHeader>
          <DialogTitle>Fix artist match</DialogTitle>
        </DialogHeader>
        <div className="relative space-y-3">
          <div className="text-sm text-zinc-400">
            Search MusicBrainz for the correct artist. Optionally provide the
            track title to improve precision.
          </div>
          <div className="flex gap-2">
            <Input
              placeholder="Artist name"
              value={artistQuery}
              onChange={(e) => setArtistQuery(e.target.value)}
              disabled={submitting}
            />
            <Input
              placeholder="Track title (optional)"
              value={trackQuery}
              onChange={(e) => setTrackQuery(e.target.value)}
              disabled={submitting}
            />
            <Button
              onClick={updateVariables}
              disabled={submitting || (artistQuery || "").trim().length === 0}
            >
              Find
            </Button>
          </div>
          {fetching ? (
            <SpinnerSpacing>
              <Spinner />
            </SpinnerSpacing>
          ) : (
            <div className="grid grid-cols-1 gap-2 max-h-96 overflow-y-auto pr-1">
              {(artists?.length ?? 0) === 0 && (
                <div className="text-xs text-zinc-500">
                  No results. Try a different spelling.
                </div>
              )}
              {artists?.map((a) => {
                const isSelected = selectedId === a.id;
                return (
                  <div
                    key={a.id}
                    className={
                      "flex items-center gap-3 justify-between rounded border p-2 transition-colors " +
                      (isSelected
                        ? "border-white/30 bg-white/[0.07]"
                        : "border-white/10 hover:bg-white/5")
                    }
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <ReleaseCoverArt
                        className="h-10 w-10 rounded"
                        srcUrl={a.images?.artistThumb ?? undefined}
                        titleForPlaceholder={a.name}
                        alt={a.name}
                      />
                      <div className="min-w-0">
                        <div className="text-sm truncate">{a.name}</div>
                        {a.disambiguation && (
                          <div className="text-[11px] text-white/60 truncate">
                            {a.disambiguation}
                          </div>
                        )}
                        <div className="text-xs text-white/60 truncate">
                          {(a.type ?? "").toString()}{" "}
                          {a.country ? `• ${a.country}` : ""}
                          {typeof a.listeners === "number"
                            ? ` • ${a.listeners.toLocaleString()} listeners`
                            : ""}
                        </div>
                        {a.lastFmArtist?.summary && (
                          <div className="text-[11px] text-white/60 line-clamp-2">
                            {a.lastFmArtist.summary}
                          </div>
                        )}
                      </div>
                    </div>
                    <Button
                      size="sm"
                      onClick={() => chooseMb(a.id)}
                      disabled={submitting}
                    >
                      Select
                    </Button>
                  </div>
                );
              })}
            </div>
          )}
          {submitting && (
            <div className="absolute inset-0 flex items-center justify-center bg-black/40 backdrop-blur-[2px] rounded-md">
              <div className="flex items-center gap-3 text-sm text-white/90">
                <Spinner />
                <span>Linking artist…</span>
              </div>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};
