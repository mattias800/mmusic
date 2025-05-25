import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useSubscription } from "urql";
import { whenTypename } from "@/common/utils/TypenameMatcher.ts";
import { ProgressIndicator } from "@/components/progress/ProgressIndicator.tsx";

export interface ArtistServerStatusProps {
  artist: FragmentType<typeof artistServerStatusArtistFragment>;
}

const artistServerStatusArtistFragment = graphql(`
  fragment ArtistServerStatus_Artist on Artist {
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
`);

const subscription = graphql(`
  subscription ArtistServerStatus($artistId: ID!) {
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

export const ArtistServerStatus: React.FC<ArtistServerStatusProps> = (
  props,
) => {
  const artist = useFragment(artistServerStatusArtistFragment, props.artist);

  useSubscription({ query: subscription, variables: { artistId: artist.id } });

  const label = whenTypename(artist.serverStatus.result)
    .is("ArtistServerStatusImportingArtist", () => "Importing artist")
    .is("ArtistServerStatusImportingArtistReleases", () => "Importing releases")
    .default(() => "Finished");

  const visible = whenTypename(artist.serverStatus.result)
    .is("ArtistServerStatusImportingArtist", () => true)
    .is("ArtistServerStatusImportingArtistReleases", () => true)
    .default(() => false);

  const progressPercent = whenTypename(artist.serverStatus.result)
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
