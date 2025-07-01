import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { ArtistList } from "@/features/artist/artist-list/ArtistList.tsx";

export interface ArtistListPageProps {}

const artistListQuery = graphql(`
  query ArtistListQuery {
    artist {
      all {
        id
        ...ArtistList_Artist
      }
    }
  }
`);

export const ArtistListPage: React.FC<ArtistListPageProps> = () => {
  const [{ error, data, fetching, stale }] = useQuery({
    query: artistListQuery,
  });

  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.artist.all) return <div>No data</div>;

  return (
    <>
      <title>Artists</title>
      <ArtistList artists={data.artist.all} />
    </>
  );
};
