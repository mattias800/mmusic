import * as React from "react";
import { Dialog, DialogHeader, DialogTitle } from "@/components/ui/dialog.tsx";
import { LargeDialogContent } from "@/components/ui/large-dialog-content.tsx";
import { Button } from "@/components/ui/button.tsx";
import { useQuery, useMutation } from "urql";
import { graphql } from "@/gql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";

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
  mutation FixMatch_SetReleaseMatchOverride(
    $input: SetReleaseMatchOverrideInput!
  ) {
    setReleaseMatchOverride(input: $input) {
      __typename
      ... on SetReleaseMatchOverrideSuccess {
        release {
          id
        }
      }
      ... on SetReleaseMatchOverrideError {
        message
      }
    }
  }
`);

const setReleaseGroupMutation = graphql(`
  mutation FixMatch_SetReleaseGroup($input: SetReleaseGroupInput!) {
    setReleaseGroup(input: $input) {
      __typename
      ... on SetReleaseGroupSuccess {
        release {
          id
        }
      }
      ... on SetReleaseGroupError {
        message
      }
    }
  }
`);

// We need the release's MusicBrainz releaseGroupId from the Release object,
// and also the artist/release titles for auto-searching other release groups.
const releaseConnectionsQuery = graphql(`
  query FixMatch_ReleaseConnections(
    $artistId: ID!
    $releaseFolderName: String!
  ) {
    serverLibrary {
      releaseForArtistByFolderName(
        artistId: $artistId
        releaseFolderName: $releaseFolderName
      ) {
        id
        musicBrainzReleaseGroupId
        title
        artist {
          id
          name
        }
      }
    }
  }
`);

// types are generated via codegen

export const FixMatchDialog: React.FC<FixMatchDialogProps> = (props) => {
  const [mode, setMode] = React.useState<"inGroup" | "searchRg">("inGroup");
  const [{ data: connectionsData }] = useQuery({
    query: releaseConnectionsQuery,
    variables: {
      artistId: props.artistId,
      releaseFolderName: props.releaseFolderName,
    },
    pause: !props.open,
  });

  const releaseGroupId = connectionsData?.serverLibrary
    ?.releaseForArtistByFolderName?.musicBrainzReleaseGroupId as
    | string
    | undefined;
  const releaseTitle = connectionsData?.serverLibrary
    ?.releaseForArtistByFolderName?.title as string | undefined;
  const artistName = connectionsData?.serverLibrary
    ?.releaseForArtistByFolderName?.artist?.name as string | undefined;

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
  const [, setReleaseGroup] = useMutation(setReleaseGroupMutation);

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

  const releaseGroupSearchQuery = graphql(`
    query FixMatch_SearchReleaseGroups(
      $name: String!
      $artistName: String!
      $limit: Int!
      $offset: Int!
    ) {
      musicBrainz {
        releaseGroup {
          searchByNameAndArtistName(
            name: $name
            artistName: $artistName
            limit: $limit
            offset: $offset
          ) {
            id
            title
            primaryType
            secondaryTypes
            firstReleaseDate
          }
        }
      }
    }
  `);

  const [{ data: rgData, fetching: rgFetching, error: rgError }] = useQuery({
    query: releaseGroupSearchQuery,
    variables: {
      name: releaseTitle || "",
      artistName: artistName || "",
      limit: 25,
      offset: 0,
    },
    pause:
      !props.open ||
      mode !== "searchRg" ||
      !artistName?.trim() ||
      !releaseTitle?.trim(),
  });

  const candidates = data?.releasesWithScores ?? [];

  return (
    <Dialog open={props.open} onOpenChange={props.onOpenChange}>
      <LargeDialogContent>
        <div className="px-6 pt-6">
          <DialogHeader className="px-0 pt-0">
            <DialogTitle>Fix match</DialogTitle>
          </DialogHeader>
        </div>
        <div className="space-y-4 overflow-hidden px-6 pb-6">
          <div className="flex gap-2 text-xs">
            <Button
              variant={mode === "inGroup" ? "default" : "ghost"}
              size="sm"
              onClick={() => setMode("inGroup")}
            >
              In this release group
            </Button>
            <Button
              variant={mode === "searchRg" ? "default" : "ghost"}
              size="sm"
              onClick={() => setMode("searchRg")}
            >
              Search other release groups
            </Button>
          </div>

          {mode === "searchRg" && (
            <div className="space-y-2">
              {rgFetching && (
                <SpinnerSpacing>
                  <SpinnerSpacing />
                </SpinnerSpacing>
              )}
              {rgError && !rgFetching && (
                <p className="text-sm text-red/70">{rgError.message}</p>
              )}
              {!rgFetching &&
                (rgData?.musicBrainz?.releaseGroup?.searchByNameAndArtistName
                  ?.length ?? 0) === 0 && (
                  <p className="text-sm text-white/70">
                    No release groups found.
                  </p>
                )}
              <div className="space-y-2 max-h-[70vh] overflow-y-auto pr-0">
                {rgData?.musicBrainz?.releaseGroup?.searchByNameAndArtistName?.map(
                  (rg) => (
                    <div
                      key={rg.id}
                      className="flex items-center justify-between gap-3 rounded-md border border-white/10 p-2 hover:bg-white/5"
                    >
                      <div className="min-w-0">
                        <div className="font-medium truncate">{rg.title}</div>
                        <div className="text-xs text-white/60 truncate">
                          {rg.primaryType}
                          {rg.secondaryTypes?.length
                            ? ` • ${rg.secondaryTypes.join(", ")}`
                            : ""}
                          {rg.firstReleaseDate
                            ? ` • ${rg.firstReleaseDate}`
                            : ""}
                        </div>
                      </div>
                      <Button
                        size="sm"
                        className="shrink-0"
                        onClick={async () => {
                          props.onBeginFix?.();
                          props.onOpenChange(false);
                          await setReleaseGroup({
                            input: {
                              artistId: props.artistId,
                              releaseFolderName: props.releaseFolderName,
                              musicBrainzReleaseGroupId: rg.id,
                            },
                          });
                          props.onEndFix?.();
                        }}
                      >
                        Use this release group
                      </Button>
                    </div>
                  ),
                )}
              </div>
            </div>
          )}

          {mode === "inGroup" && !releaseGroupId && (
            <p className="text-sm text-white/70">
              Missing MusicBrainz release group connection for this album.
            </p>
          )}
          {mode === "inGroup" && fetching && (
            <p className="text-sm text-white/70">Loading candidates…</p>
          )}
          {mode === "inGroup" && error && (
            <p className="text-sm text-red-400">{error.message}</p>
          )}
          {mode === "inGroup" &&
            !fetching &&
            releaseGroupId &&
            candidates.length === 0 && (
              <p className="text-sm text-white/70">No candidates found.</p>
            )}

          {mode === "inGroup" && (
            <div className="space-y-2 max-h-[70vh] overflow-y-auto overflow-x-hidden pr-0">
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
                        <div className="font-medium truncate">
                          {c.release.title}
                        </div>
                        <div className="text-xs text-white/60">
                          {c.release.year ?? ""}{" "}
                          {c.release.country ? `• ${c.release.country}` : ""}{" "}
                          {c.release.status ? `• ${c.release.status}` : ""} •{" "}
                          {c.release.media?.[0]?.tracks?.length ?? 0} tracks
                        </div>
                      </div>
                      <div className="text-xs text-white/70">
                        Score: {c.score}
                      </div>
                    </div>
                    {c.reasons?.length > 0 && (
                      <div className="mt-1 text-[11px] text-white/50 truncate">
                        {c.reasons.join(" • ")}
                      </div>
                    )}
                  </div>
                  <Button
                    size="sm"
                    className="shrink-0"
                    onClick={() => onChoose(c.release.id)}
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
      </LargeDialogContent>
    </Dialog>
  );
};
