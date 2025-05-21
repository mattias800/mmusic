import { graphql } from "@/gql";
import { useQuery } from "urql";
import { useParams } from "react-router";
import { ArtistPanel } from "@/features/artist/artist-page/ArtistPanel.tsx";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";

export const artistQuery = graphql(`
  query ArtistQuery($artistId: ID!) {
    artist {
      byId(id: $artistId) {
        id
        ...ArtistPanel_Artist
      }
    }
  }
`);

export const ArtistPage = () => {
  const { artistId } = useParams<{ artistId: string }>();
  const [{ error, data, fetching, stale }] = useQuery({
    query: artistQuery,
    variables: { artistId: artistId! },
    pause: !artistId,
  });

  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.artist.byId) return <div>No data</div>;

  return <ArtistPanel artist={data.artist.byId} />;
};
