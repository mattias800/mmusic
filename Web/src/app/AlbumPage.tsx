import { graphql } from "@/gql";
import { useQuery } from "urql";
import { useParams } from "react-router";
import { AlbumPanel } from "@/features/album/AlbumPanel.tsx";
import { PageLoading, PageError, PageNoData } from "@/components/ui";
import { Music, AlertTriangle, Disc3 } from "lucide-react";

const albumQuery = graphql(`
  query AlbumQuery($artistId: ID!, $releaseFolderName: String!) {
    serverLibrary {
      artistById(id: $artistId) {
        id
        name
        releaseByFolderName(releaseFolderName: $releaseFolderName) {
          id
          title
          ...AlbumPanel_Release
        }
      }
    }
  }
`);

export const AlbumPage = () => {
  const { artistId, releaseFolderName } = useParams<{
    artistId: string;
    releaseFolderName: string;
  }>();
  const [{ error, data, fetching, stale }] = useQuery({
    query: albumQuery,
    variables: { artistId: artistId!, releaseFolderName: releaseFolderName! },
    pause: !artistId || !releaseFolderName,
  });

  if (fetching || stale) return <PageLoading 
    title="Loading Album" 
    subtitle="Fetching album information and track listing"
    icon={Music}
    iconBgColor="bg-purple-500/20"
  />;
  
  if (error) return <PageError 
    title="Failed to Load Album" 
    message="We couldn't load the album information"
    error={error}
    icon={AlertTriangle}
    iconBgColor="bg-red-500/20"
  />;
  
  const release = data?.serverLibrary.artistById?.releaseByFolderName;

  if (!release) return <PageNoData 
    title="Album Not Found" 
    message="The album you're looking for doesn't exist or may have been removed"
    icon={Disc3}
    iconBgColor="bg-yellow-500/20"
  />;

  return (
    <>
      <title>{release.title}</title>
      <AlbumPanel release={release} />
    </>
  );
};
