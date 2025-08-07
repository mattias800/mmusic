import { graphql } from "@/gql";
import * as React from "react";
import { byStringField } from "@/common/sorting/Comparators.ts";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";

export interface ArtistSingleListProps {
  artistId: string;
}

const artistSingleListArtistQuery = graphql(`
  query ArtistSingleList($artistId: ID!) {
    serverLibrary {
      artistById(id: $artistId) {
        id
        singles {
          id
          firstReleaseDate
          ...AlbumCard_Release
        }
      }
    }
  }
`);

export const ArtistSingleList: React.FC<ArtistSingleListProps> = (props) => {
  const [{ data, fetching }] = useQuery({
    query: artistSingleListArtistQuery,
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
      {data.artist.byId.singles
        .toSorted(byStringField((a) => a.firstReleaseDate ?? ""))
        .toReversed()
        .map((release) => (
          <AlbumCard releaseGroup={release} key={release.id} />
        ))}
    </div>
  );
};
