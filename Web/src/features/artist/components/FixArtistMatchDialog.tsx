import * as React from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog.tsx";
import { Button } from "@/components/ui/button.tsx";
import { useMutation, useQuery } from "urql";
import { graphql } from "@/gql";

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
    props.onBeginFix?.();
    props.onOpenChange(false);
    await setMbMatch({
      input: { artistId: props.artistId, musicBrainzArtistId: mbId },
    });
    props.onEndFix?.();
  };

  const onSelectSpotify = async (spId: string) => {
    props.onBeginFix?.();
    props.onOpenChange(false);
    await setSpMatch({
      input: { artistId: props.artistId, spotifyArtistId: spId },
    });
    props.onEndFix?.();
  };

  return (
    <Dialog open={props.open} onOpenChange={props.onOpenChange}>
      <DialogContent className="max-w-2xl overflow-hidden">
        <DialogHeader>
          <DialogTitle>Fix artist match</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 overflow-hidden">
          <div className="flex gap-2 text-xs">
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
            <div className="space-y-2 max-h-96 overflow-y-auto pr-2">
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
                <div
                  key={a.id}
                  className="flex items-center justify-between gap-3 rounded-md border border-white/10 p-2 hover:bg-white/5"
                >
                  <div className="min-w-0">
                    <div className="font-medium truncate">{a.name}</div>
                    <div className="text-xs text-white/60 truncate">
                      {a.type ?? ""} {a.country ? `• ${a.country}` : ""}
                    </div>
                  </div>
                  <Button
                    size="sm"
                    className="shrink-0"
                    onClick={() => onSelectMb(a.id)}
                  >
                    Select
                  </Button>
                </div>
              ))}
            </div>
          )}

          {mode === "spotify" && (
            <div className="space-y-2 max-h-96 overflow-y-auto pr-2">
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
                <div
                  key={a.id}
                  className="flex items-center justify-between gap-3 rounded-md border border-white/10 p-2 hover:bg-white/5"
                >
                  <div className="flex items-center gap-3 min-w-0">
                    {a.images?.[0]?.url && (
                      <img
                        src={a.images[0].url}
                        className="h-10 w-10 rounded object-cover"
                        alt="cover"
                      />
                    )}
                    <div className="min-w-0">
                      <div className="font-medium truncate">{a.name}</div>
                    </div>
                  </div>
                  <Button
                    size="sm"
                    className="shrink-0"
                    onClick={() => onSelectSpotify(a.id)}
                  >
                    Select
                  </Button>
                </div>
              ))}
            </div>
          )}

          <div className="flex justify-end gap-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => props.onOpenChange(false)}
            >
              Close
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};
