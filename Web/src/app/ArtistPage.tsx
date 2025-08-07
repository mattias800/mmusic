import { graphql } from "@/gql";
import { useQuery } from "urql";
import { useParams } from "react-router";
import { ArtistPanel } from "@/features/artist/artist-page/ArtistPanel.tsx";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { ArtistNotFound } from "@/app/ArtistNotFound.tsx";

const artistQuery = graphql(`
  query ArtistQuery($artistId: ID!) {
    serverLibrary {
      artistById(id: $artistId) {
        id
        name
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

  if (!artistId) {
    return "Invalid artist ID";
  }

  if (fetching || stale) {
    return <ScreenSpinner />;
  }
  if (error) {
    return <div>Error: {error.message}</div>;
  }
  if (!data?.artist.byId) {
    return <ArtistNotFound />;
  }

  return (
    <>
      <title>{data.artist.byId.name}</title>
      <ArtistPanel artist={data.artist.byId} />
    </>
  );
};
