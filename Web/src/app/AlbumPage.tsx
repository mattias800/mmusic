import { graphql } from "@/gql";
import { useQuery } from "urql";
import { useParams } from "react-router";
import { AlbumPanel } from "@/features/album/AlbumPanel.tsx";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";

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

  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  const release = data?.serverLibrary.artistById?.releaseByFolderName;

  if (!release) return <div>No data</div>;

  return (
    <>
      <title>{release.title}</title>
      <AlbumPanel release={release} />
    </>
  );
};
