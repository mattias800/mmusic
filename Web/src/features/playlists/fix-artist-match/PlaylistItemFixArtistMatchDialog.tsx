import * as React from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog.tsx";
import { Input } from "@/components/ui/input.tsx";
import { Button } from "@/components/ui/button.tsx";
import { graphql } from "@/gql";
import { useMutation, useQuery } from "urql";
import { useState } from "react";
import { Spinner } from "@/components/spinner/Spinner.tsx";

const setMatchMutation = graphql(`
  mutation SetPlaylistItemArtistMatch($input: SetPlaylistItemArtistMatchInput!) {
    setPlaylistItemArtistMatch(input: $input) {
      __typename
      ... on SetPlaylistItemArtistMatchSuccess {
        playlistItem { id }
      }
      ... on SetPlaylistItemArtistMatchNotFound { message }
    }
  }
`);

const externalArtistSearchQuery = graphql(`
  query FixArtist_ExternalArtistSearch($searchTerm: String!, $service: ExternalServiceType!) {
    artist {
      searchExternalArtists(searchTerm: $searchTerm, serviceType: $service, limit: 10) {
        artistId
        artistName
        service
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
}

export const PlaylistItemFixArtistMatchDialog: React.FC<PlaylistItemFixArtistMatchDialogProps> = (props) => {
  const [query, setQuery] = useState(props.initialArtistQuery ?? "");
  const [searchTerm, setSearchTerm] = useState(props.initialArtistQuery ?? "");
  const [{ data, fetching }] = useQuery({
    query: externalArtistSearchQuery,
    variables: { searchTerm, service: "SPOTIFY" },
    pause: !props.open || searchTerm.trim().length === 0,
  });
  const [, setMatch] = useMutation(setMatchMutation);

  const search = () => setSearchTerm(query);

  const choose = async (externalArtistId: string) => {
    await setMatch({ input: { playlistId: props.playlistId, playlistItemId: props.playlistItemId, externalArtistId } });
    props.onOpenChange(false);
  };

  return (
    <Dialog open={props.open} onOpenChange={props.onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Fix artist match</DialogTitle>
        </DialogHeader>
        <div className="space-y-3">
          <div className="text-sm text-zinc-400">Search Spotify artist and link this playlist item to the correct artist.</div>
          <div className="flex gap-2">
            <Input placeholder="Spotify artist URL or ID" value={query} onChange={(e) => setQuery(e.target.value)} />
            <Button onClick={search}>Find</Button>
          </div>
          {fetching ? (
            <Spinner />
          ) : (
            <div className="space-y-2">
              {(data?.artist?.searchExternalArtists?.length ?? 0) === 0 && (
                <div className="text-xs text-zinc-500">No results. Try a different spelling.</div>
              )}
              {data?.artist?.searchExternalArtists?.map((a) => (
                <div key={a.artistId} className="flex items-center justify-between rounded border p-2">
                  <div className="text-sm">{a.artistName}</div>
                  <Button size="sm" onClick={() => choose(a.artistId)}>Select</Button>
                </div>
              ))}
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};


