import { graphql } from "@/gql";
import * as React from "react";
import { useQuery } from "urql";
import { useParams } from "react-router";
import { ArtistPanel } from "@/features/artist/ArtistPanel";

export interface ArtistProps {}

export const albumQuery = graphql(`
  query AlbumQuery($artistId: ID!) {
    artist {
      byId(id: $artistId) {
        id
      }
    }
  }
`);

export const Artist: React.FC<ArtistProps> = () => {
  const { artistId } = useParams<{ artistId: string }>();
  const [{ error, data, fetching }] = useQuery({
    query: albumQuery,
    variables: { artistId: artistId! },
    pause: true,
  });

  // if (fetching) return <div>Loading...</div>;
  // if (error) return <div>Error: {error.message}</div>;
  // if (!data?.release.byId) return <div>No data</div>;

  return <ArtistPanel />;
};
