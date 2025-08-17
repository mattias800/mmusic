import { graphql } from "@/gql";
import { useQuery } from "urql";
import { useParams } from "react-router";
import { ArtistPanel } from "@/features/artist/artist-page/ArtistPanel.tsx";
import { PageLoading, PageError } from "@/components/ui";
import { ArtistNotFound } from "@/app/ArtistNotFound.tsx";
import { Music, AlertTriangle } from "lucide-react";

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
    return <PageError 
      title="Invalid Artist ID" 
      message="The artist ID provided is not valid"
      icon={AlertTriangle}
      iconBgColor="bg-red-500/20"
    />;
  }

  if (fetching || stale) {
    return <PageLoading 
      title="Loading Artist" 
      subtitle="Fetching artist information and discography"
      icon={Music}
      iconBgColor="bg-purple-500/20"
    />;
  }
  
  if (error) {
    return <PageError 
      title="Failed to Load Artist" 
      message="We couldn't load the artist information"
      error={error}
      icon={AlertTriangle}
      iconBgColor="bg-red-500/20"
    />;
  }
  
  if (!data?.serverLibrary.artistById) {
    return <ArtistNotFound />;
  }

  return (
    <>
      <title>{data.serverLibrary.artistById.name}</title>
      <ArtistPanel artist={data.serverLibrary.artistById} />
    </>
  );
};
