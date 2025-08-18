import * as React from "react";
import { graphql } from "@/gql";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { byStringField } from "@/common/sorting/Comparators.ts";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";
import { useQuery } from "urql";
import { ReleaseType } from "@/gql/graphql.ts";

export interface ReleaseListProps {
  artistId: string;
  releaseType: ReleaseType;
}

const releaseListQuery = graphql(`
  query ReleaseListQuery($artistId: ID!) {}
    artist {
      byId(artistId: $artistId) {
        id
        releases {
          id
          firstReleaseDate
          type
          ...AlbumCard_Release
        }
      }
    }
  }
`);

export const ReleaseList: React.FC<ReleaseListProps> = ({ artistId }) => {
  const [{ fetching, error, data }] = useQuery({
    query: releaseListQuery,
    variables: { artistId },
  });

  if (fetching) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>Error: {error.message}</div>;
  }

  if (!data?.artist.byId) {
    return <div>No data..</div>;
  }

  return (
    <CardFlexList>
      {data.artist.eps
        .toSorted(byStringField((a) => a.firstReleaseDate ?? ""))
        .toReversed()
        .map((release) => (
          <AlbumCard release={release} key={release.id} />
        ))}
    </CardFlexList>
  );
};
