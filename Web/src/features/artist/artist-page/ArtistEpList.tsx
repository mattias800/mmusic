import { graphql } from "@/gql";
import * as React from "react";
import { byStringField } from "@/common/sorting/Comparators.ts";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";

export interface ArtistEpListProps {
  artistId: string;
}

const artistEpListArtistQuery = graphql(`
  query ArtistEpList($artistId: ID!) {
    artist {
      byId(id: $artistId) {
        id
        eps {
          id
          firstReleaseDate
          ...AlbumCard_ReleaseGroup
        }
      }
    }
  }
`);

export const ArtistEpList: React.FC<ArtistEpListProps> = (props) => {
  const [{ data, fetching }] = useQuery({
    query: artistEpListArtistQuery,
    variables: { artistId: props.artistId },
  });

  if (fetching) {
    return (
      <SpinnerSpacing>
        <Spinner />
      </SpinnerSpacing>
    );
  }

  if (!data?.artist.byId) {
    return <div>No data..</div>;
  }
  return (
    <div className={"flex flex-wrap gap-8"}>
      {data.artist.byId.eps
        .toSorted(byStringField((a) => a.firstReleaseDate ?? ""))
        .toReversed()
        .map((release) => (
          <AlbumCard releaseGroup={release} key={release.id} />
        ))}
    </div>
  );
};
