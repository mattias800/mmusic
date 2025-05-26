import * as React from "react";
import { graphql } from "@/gql";
import { useQuery, useSubscription } from "urql";
import { whenTypename } from "@/common/utils/TypenameMatcher.ts";
import { ProgressIndicator } from "@/components/progress/ProgressIndicator.tsx";

export interface ArtistServerStatusProps {
  artistId: string;
}

const query = graphql(`
  query ArtistServerStatus($artistId: ID!) {
    artist {
      byId(id: $artistId) {
        id
        serverStatus {
          id
          result {
            __typename
            ... on ArtistServerStatusResultBase {
              releasesVisible
              topTracksVisible
            }
            ... on ArtistServerStatusImportingArtistReleases {
              totalNumReleaseGroupsBeingImported
              numReleaseGroupsFinishedImporting
            }
          }
        }
      }
    }
  }
`);

const subscription = graphql(`
  subscription ArtistServerStatusSub($artistId: ID!) {
    artistServerStatusUpdated(artistId: $artistId) {
      id
      result {
        __typename
        ... on ArtistServerStatusResultBase {
          releasesVisible
          topTracksVisible
        }
        ... on ArtistServerStatusImportingArtistReleases {
          totalNumReleaseGroupsBeingImported
          numReleaseGroupsFinishedImporting
        }
      }
    }
  }
`);

export const ArtistServerStatus: React.FC<ArtistServerStatusProps> = ({
  artistId,
}) => {
  const [{ data }] = useQuery({
    query,
    variables: { artistId },
    requestPolicy: "cache-and-network",
  });

  useSubscription({ query: subscription, variables: { artistId } });

  const serverStatus = data?.artist.byId?.serverStatus;

  const label = whenTypename(serverStatus?.result)
    .is("ArtistServerStatusImportingArtist", () => "Importing artist")
    .is("ArtistServerStatusImportingArtistReleases", () => "Importing releases")
    .default(() => "Finished");

  const visible = whenTypename(serverStatus?.result)
    .is("ArtistServerStatusImportingArtist", () => true)
    .is("ArtistServerStatusImportingArtistReleases", () => true)
    .default(() => false);

  const progressPercent = whenTypename(serverStatus?.result)
    .is(
      "ArtistServerStatusImportingArtistReleases",
      (p) =>
        (p.numReleaseGroupsFinishedImporting /
          p.totalNumReleaseGroupsBeingImported) *
        100,
    )
    .default(() => 100);

  return (
    <div
      className={`flex flex-col gap-4 rounded-md bg-black/70 p-4 w-64 transition-all duration-300 ${visible ? "opacity-100" : "opacity-0"}`}
    >
      <span className={"font-medium"}>{label}</span>
      {progressPercent && (
        <ProgressIndicator progressPercent={progressPercent} />
      )}
    </div>
  );
};
