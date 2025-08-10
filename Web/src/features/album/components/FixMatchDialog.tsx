import * as React from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog.tsx";
import { Button } from "@/components/ui/button.tsx";
import { useQuery, useMutation } from "urql";
import { graphql } from "@/gql";

interface FixMatchDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  artistId: string;
  releaseFolderName: string;
  onBeginFix?: () => void;
  onEndFix?: () => void;
}

// Query candidates with scores
const releasesWithScoresQuery = graphql(`
  query FixMatch_ReleasesWithScores(
    $releaseGroupId: String!
    $artistId: String!
    $releaseFolderName: String!
  ) {
    releasesWithScores(
      releaseGroupId: $releaseGroupId
      artistId: $artistId
      releaseFolderName: $releaseFolderName
    ) {
      score
      reasons
      release {
        id
        title
        coverArtUri
        year
        country
        status
        media {
          tracks {
            id
          }
        }
      }
    }
  }
`);

// Mutation to set override
const setOverrideMutation = graphql(`
  mutation FixMatch_SetReleaseMatchOverride($input: SetReleaseMatchOverrideInput!) {
    setReleaseMatchOverride(input: $input) {
      __typename
      ... on SetReleaseMatchOverrideSuccess {
        release { id }
      }
      ... on SetReleaseMatchOverrideError {
        message
      }
    }
  }
`);

// We need the release's MusicBrainz releaseGroupId from the Release object.
// Add a tiny helper query to fetch it via the AlbumPanel fragment constraints.
const releaseConnectionsQuery = graphql(`
  query FixMatch_ReleaseConnections($artistId: ID!, $releaseFolderName: String!) {
    serverLibrary {
      releaseForArtistByFolderName(
        artistId: $artistId
        releaseFolderName: $releaseFolderName
      ) {
        id
        musicBrainzReleaseGroupId
      }
    }
  }
`);

// types are generated via codegen

export const FixMatchDialog: React.FC<FixMatchDialogProps> = (props) => {
  const [{ data: connectionsData }] = useQuery({
    query: releaseConnectionsQuery,
    variables: {
      artistId: props.artistId,
      releaseFolderName: props.releaseFolderName,
    },
    pause: !props.open,
  });

  const releaseGroupId = connectionsData?.serverLibrary?.releaseForArtistByFolderName?.musicBrainzReleaseGroupId as string | undefined;

  const [{ data, fetching, error }] = useQuery({
    query: releasesWithScoresQuery,
    variables: {
      releaseGroupId: releaseGroupId ?? "",
      artistId: props.artistId,
      releaseFolderName: props.releaseFolderName,
    },
    pause: !props.open || !releaseGroupId,
  });

  const [, setOverride] = useMutation(setOverrideMutation);

  const onChoose = async (mbReleaseId: string) => {
    // start loading immediately and close dialog
    props.onBeginFix?.();
    props.onOpenChange(false);

    await setOverride({
      input: {
        artistId: props.artistId,
        releaseFolderName: props.releaseFolderName,
        musicBrainzReleaseId: mbReleaseId,
      },
    });
    props.onEndFix?.();
  };

  const candidates = data?.releasesWithScores ?? [];

  return (
    <Dialog open={props.open} onOpenChange={props.onOpenChange}>
      <DialogContent className="max-w-2xl overflow-hidden">
        <DialogHeader>
          <DialogTitle>Fix match</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 overflow-hidden">
          {!releaseGroupId && (
            <p className="text-sm text-white/70">
              Missing MusicBrainz release group connection for this album.
            </p>
          )}
          {fetching && <p className="text-sm text-white/70">Loading candidates…</p>}
          {error && (
            <p className="text-sm text-red-400">{error.message}</p>
          )}
          {!fetching && releaseGroupId && candidates.length === 0 && (
            <p className="text-sm text-white/70">No candidates found.</p>
          )}

          <div className="space-y-2 max-h-96 overflow-y-auto overflow-x-hidden pr-2">
            {candidates.map((c) => (
              <div
                key={c.release.id}
                className="flex w-full items-start gap-3 rounded-md border border-white/10 p-2 hover:bg-white/5"
              >
                <img
                  src={c.release.coverArtUri}
                  className="h-14 w-14 rounded object-cover"
                  alt="cover"
                />
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between gap-3">
                    <div className="truncate">
                      <div className="font-medium truncate">{c.release.title}</div>
                      <div className="text-xs text-white/60">
                        {c.release.year ?? ""} {c.release.country ? `• ${c.release.country}` : ""} {c.release.status ? `• ${c.release.status}` : ""}
                        {" "}• {c.release.media?.[0]?.tracks?.length ?? 0} tracks
                      </div>
                    </div>
                    <div className="text-xs text-white/70">Score: {c.score}</div>
                  </div>
                  {c.reasons?.length > 0 && (
                    <div className="mt-1 text-[11px] text-white/50 truncate">
                      {c.reasons.join(" • ")}
                    </div>
                  )}
                </div>
                <Button size="sm" className="shrink-0" onClick={() => onChoose(c.release.id)}>Select</Button>
              </div>
            ))}
          </div>

          <div className="flex justify-end gap-2">
            <Button variant="ghost" size="sm" onClick={() => props.onOpenChange(false)}>
              Close
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

