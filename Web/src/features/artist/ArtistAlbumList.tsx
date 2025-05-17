import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";
import { byStringField } from "@/common/sorting/Comparators.ts";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";

export interface ArtistAlbumListProps {
  artistId: string;
}

export const artistAlbumListQuery = graphql(`
  query ArtistAlbumList($artistId: ID!) {
    artist {
      byId(id: $artistId) {
        id
        albums {
          id
          firstReleaseDate
          ...AlbumCard_ReleaseGroup
        }
      }
    }
  }
`);

export const ArtistAlbumList: React.FC<ArtistAlbumListProps> = (props) => {
  const [{ data, fetching }] = useQuery({
    query: artistAlbumListQuery,
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
      {data.artist.byId.albums
        .toSorted(byStringField((a) => a.firstReleaseDate ?? ""))
        .toReversed()
        .map((release) => (
          <AlbumCard releaseGroup={release} key={release.id} />
        ))}
    </div>
  );
};
