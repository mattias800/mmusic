import * as React from "react";
import { useEffect, useMemo, useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog.tsx";
import { Input } from "@/components/ui/input.tsx";
import { Button } from "@/components/ui/button.tsx";
import { graphql } from "@/gql";
import { useMutation, useQuery } from "urql";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";

const mbArtistSearchQuery = graphql(`
  query FixArtist_MbArtistSearch($artistName: String!, $trackName: String!) {
    musicBrainz {
      artist {
        searchByName(name: $artistName, limit: 15) {
          id
          name
          disambiguation
          images {
            artistThumb
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

const setMbMatchMutation = graphql(`
  mutation FixArtist_SetPlaylistItemArtistMusicBrainzMatch(
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
  initialArtistQuery?: string;
  initialTrackQuery?: string;
}

export const PlaylistItemFixArtistMatchDialog: React.FC<
  PlaylistItemFixArtistMatchDialogProps
> = (props) => {
  const [artistQuery, setArtistQuery] = useState(
    props.initialArtistQuery ?? "",
  );
  const [trackQuery, setTrackQuery] = useState<string>(
    props.initialTrackQuery ?? "",
  );
  const [searchVars, setSearchVars] = useState<{
    artistName: string;
    trackName: string;
  } | null>(null);
  const [{ data, fetching }] = useQuery({
    query: mbArtistSearchQuery,
    variables: searchVars ?? { artistName: "", trackName: "" },
    pause:
      !props.open || !searchVars || searchVars.artistName.trim().length === 0,
  });
  const [, setMbMatch] = useMutation(setMbMatchMutation);

  const search = () =>
    setSearchVars({
      artistName: artistQuery,
      trackName: trackQuery || artistQuery,
    });

  // Auto-trigger search once when dialog opens, using current input values
  useEffect(() => {
    if (!props.open) return;
    const artist = (artistQuery || "").trim();
    if (artist.length === 0) return;
    setSearchVars({ artistName: artist, trackName: trackQuery || artist });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [props.open]);

  // If track query present, bias artist list to those appearing on matching recordings
  const biasedArtistIds = useMemo(() => {
    const recs = data?.musicBrainz?.recording?.searchByName ?? [];
    const ids = new Set<string>();
    recs.forEach((r) => r.artists?.forEach((a) => a?.id && ids.add(a.id)));
    return ids;
  }, [data]);

  const artists = useMemo(() => {
    const all = data?.musicBrainz?.artist?.searchByName ?? [];
    if (!biasedArtistIds.size) return all;
    return [...all].sort((a, b) => {
      const aBias = biasedArtistIds.has(a.id) ? 1 : 0;
      const bBias = biasedArtistIds.has(b.id) ? 1 : 0;
      return bBias - aBias;
    });
  }, [data, biasedArtistIds]);

  const chooseMb = async (musicBrainzArtistId: string) => {
    await setMbMatch({
      input: {
        playlistId: props.playlistId,
        playlistItemId: props.playlistItemId,
        musicBrainzArtistId,
      },
    });
    props.onOpenChange(false);
  };

  return (
    <Dialog open={props.open} onOpenChange={props.onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Fix artist match</DialogTitle>
        </DialogHeader>
        <div className="space-y-3">
          <div className="text-sm text-zinc-400">
            Search MusicBrainz for the correct artist. Optionally provide the
            track title to improve precision.
          </div>
          <div className="flex gap-2">
            <Input
              placeholder="Artist name"
              value={artistQuery}
              onChange={(e) => setArtistQuery(e.target.value)}
            />
            <Input
              placeholder="Track title (optional)"
              value={trackQuery}
              onChange={(e) => setTrackQuery(e.target.value)}
            />
            <Button onClick={search}>Find</Button>
          </div>
          {fetching ? (
            <Spinner />
          ) : (
            <div className="grid grid-cols-1 gap-2 max-h-96 overflow-y-auto pr-1">
              {(artists?.length ?? 0) === 0 && (
                <div className="text-xs text-zinc-500">
                  No results. Try a different spelling.
                </div>
              )}
              {artists?.map((a) => (
                <div
                  key={a.id}
                  className="flex items-center gap-3 justify-between rounded border border-white/10 p-2 hover:bg-white/5"
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
                    </div>
                  </div>
                  <Button size="sm" onClick={() => chooseMb(a.id)}>
                    Select
                  </Button>
                </div>
              ))}
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};
