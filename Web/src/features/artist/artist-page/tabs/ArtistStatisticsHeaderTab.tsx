import * as React from "react";
import { ArtistStatisticsHeader } from "@/features/artist/artist-page/ArtistStatisticsHeader.tsx";
import { useParams } from "react-router";
import { useQuery } from "urql";
import { graphql } from "@/gql";

export interface ArtistStatisticsHeaderTabProps {}

const artistStatisticsHeaderTabQuery = graphql(`
  query ArtistStatisticsHeader($artistId: ID!) {
    artist {
      byId(artistId: $artistId) {
        id
        ...ArtistStatisticsHeader_Artist
      }
    }
  }
`);

export const ArtistStatisticsHeaderTab: React.FC<
  ArtistStatisticsHeaderTabProps
> = () => {
  const { artistId } = useParams<{ artistId: string }>();

  const [{ fetching, error, data }] = useQuery({
    query: artistStatisticsHeaderTabQuery,
    variables: { artistId: artistId ?? "" },
    pause: !artistId,
  });

  if (fetching) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>Error: {error.message}</div>;
  }

  if (!data?.artist?.byId) {
    return <div>No data..</div>;
  }

  return <ArtistStatisticsHeader artist={data.artist.byId} />;
};
