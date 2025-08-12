import * as React from "react";
import { Dialog, DialogHeader, DialogTitle } from "@/components/ui/dialog.tsx";
import { LargeDialogContent } from "@/components/ui/large-dialog-content.tsx";
import { Button } from "@/components/ui/button.tsx";
import { useMutation, useQuery } from "urql";
import { graphql } from "@/gql";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { ArtistMatchListItem } from "@/features/artist/components/ArtistMatchListItem.tsx";

interface FixArtistMatchDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  artistId: string; // local folder id
  artistName: string; // current display name
  onBeginFix?: () => void;
  onEndFix?: () => void;
}

const searchMbArtistsQuery = graphql(`
  query FixArtistMatch_SearchMbArtists(
    $name: String!
    $limit: Int!
    $offset: Int!
  ) {
    musicBrainz {
      artist {
        searchByName(name: $name, limit: $limit, offset: $offset) {
          id
          name
          country
          type
          disambiguation
          listeners
          images {
            artistThumb
          }
          lastFmArtist {
            summary
          }
        }
      }
    }
  }
`);

const setMbMatchMutation = graphql(`
  mutation FixArtistMatch_SetMb($input: SetArtistMusicBrainzMatchInput!) {
    setArtistMusicBrainzMatch(input: $input) {
      __typename
      ... on SetArtistMusicBrainzMatchSuccess {
        artist {
          id
        }
      }
      ... on SetArtistMusicBrainzMatchError {
        message
      }
    }
  }
`);

const setSpotifyMatchMutation = graphql(`
  mutation FixArtistMatch_SetSpotify($input: SetArtistSpotifyMatchInput!) {
    setArtistSpotifyMatch(input: $input) {
      __typename
      ... on SetArtistSpotifyMatchSuccess {
        artist {
          id
        }
      }
      ... on SetArtistSpotifyMatchError {
        message
      }
    }
  }
`);

const searchSpotifyArtistsQuery = graphql(`
  query FixArtistMatch_SearchSpotify(
    $name: String!
    $limit: Int!
    $offset: Int!
  ) {
    spotify {
      searchByName(name: $name, limit: $limit, offset: $offset) {
        id
        name
        images {
          url
        }
      }
    }
  }
`);

export const FixArtistMatchDialog: React.FC<FixArtistMatchDialogProps> = (
  props,
) => {
  const [mode, setMode] = React.useState<"mb" | "spotify">("mb");
  const [selectedId, setSelectedId] = React.useState<string | null>(null);
  const [submitting, setSubmitting] = React.useState(false);

  const [{ data: mbData, fetching: mbFetching, error: mbError }] = useQuery({
    query: searchMbArtistsQuery,
    variables: { name: props.artistName, limit: 25, offset: 0 },
    pause: !props.open || mode !== "mb" || !props.artistName.trim(),
  });
  const [{ data: spData, fetching: spFetching, error: spError }] = useQuery({
    query: searchSpotifyArtistsQuery,
    variables: { name: props.artistName, limit: 25, offset: 0 },
    pause: !props.open || mode !== "spotify" || !props.artistName.trim(),
  });

  const [, setMbMatch] = useMutation(setMbMatchMutation);
  const [, setSpMatch] = useMutation(setSpotifyMatchMutation);

  const onSelectMb = async (mbId: string) => {
    setSelectedId(mbId);
    setSubmitting(true);
    props.onBeginFix?.();
    await setMbMatch({
      input: { artistId: props.artistId, musicBrainzArtistId: mbId },
    });
    setSubmitting(false);
    props.onOpenChange(false);
    props.onEndFix?.();
  };

  const onSelectSpotify = async (spId: string) => {
    setSelectedId(spId);
    setSubmitting(true);
    props.onBeginFix?.();
    await setSpMatch({
      input: { artistId: props.artistId, spotifyArtistId: spId },
    });
    setSubmitting(false);
    props.onOpenChange(false);
    props.onEndFix?.();
  };

  return (
    <Dialog open={props.open} onOpenChange={props.onOpenChange}>
      <LargeDialogContent>
        <div className="px-6 pt-6">
          <DialogHeader className="px-0 pt-0">
            <DialogTitle>Fix artist match</DialogTitle>
          </DialogHeader>
        </div>
        <div className="space-y-4 overflow-hidden">
          <div className="flex gap-2 text-xs px-6 pb-6">
            <Button
              variant={mode === "mb" ? "default" : "ghost"}
              size="sm"
              onClick={() => setMode("mb")}
            >
              MusicBrainz
            </Button>
            <Button
              variant={mode === "spotify" ? "default" : "ghost"}
              size="sm"
              onClick={() => setMode("spotify")}
            >
              Spotify
            </Button>
          </div>

          {mode === "mb" && (
            <div className="relative space-y-2 max-h-[70vh] overflow-y-auto px-6 pb-6">
              {mbFetching && (
                <p className="text-sm text-white/70">Searching…</p>
              )}
              {mbError && (
                <p className="text-sm text-red-400">{mbError.message}</p>
              )}
              {!mbFetching &&
                (mbData?.musicBrainz?.artist?.searchByName?.length ?? 0) ===
                  0 && (
                  <p className="text-sm text-white/70">
                    No MusicBrainz artists found.
                  </p>
                )}
              {mbData?.musicBrainz?.artist?.searchByName?.map((a) => (
                <ArtistMatchListItem
                  key={a.id}
                  imageUrl={a.images?.artistThumb ?? undefined}
                  name={a.name}
                  disambiguation={a.disambiguation ?? undefined}
                  typeLabel={a.type ?? undefined}
                  country={a.country ?? undefined}
                  listeners={a.listeners}
                  summary={a.lastFmArtist?.summary ?? undefined}
                  selected={selectedId === a.id}
                  disabled={submitting}
                  onSelect={() => onSelectMb(a.id)}
                />
              ))}
              {submitting && (
                <div className="absolute inset-0 flex items-center justify-center bg-black/40 backdrop-blur-[2px] rounded-md">
                  <div className="flex items-center gap-3 text-sm text-white/90">
                    <Spinner />
                    <span>Linking artist…</span>
                  </div>
                </div>
              )}
            </div>
          )}

          {mode === "spotify" && (
            <div className="relative space-y-2 max-h-[70vh] overflow-y-auto px-6 pb-6">
              {spFetching && (
                <p className="text-sm text-white/70">Searching…</p>
              )}
              {spError && (
                <p className="text-sm text-red-400">{spError.message}</p>
              )}
              {!spFetching &&
                (spData?.spotify?.searchByName?.length ?? 0) === 0 && (
                  <p className="text-sm text-white/70">
                    No Spotify artists found.
                  </p>
                )}
              {spData?.spotify?.searchByName?.map((a) => (
                <ArtistMatchListItem
                  key={a.id}
                  imageUrl={a.images?.[0]?.url ?? undefined}
                  name={a.name}
                  selected={selectedId === a.id}
                  disabled={submitting}
                  onSelect={() => onSelectSpotify(a.id)}
                />
              ))}
              {submitting && (
                <div className="absolute inset-0 flex items-center justify-center bg-black/40 backdrop-blur-[2px] rounded-md">
                  <div className="flex items-center gap-3 text-sm text-white/90">
                    <Spinner />
                    <span>Linking artist…</span>
                  </div>
                </div>
              )}
            </div>
          )}

          <div className="flex justify-end gap-2 px-6 pb-4">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => props.onOpenChange(false)}
            >
              Close
            </Button>
          </div>
        </div>
      </LargeDialogContent>
    </Dialog>
  );
};
