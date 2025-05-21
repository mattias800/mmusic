import { graphql } from "@/gql";
import { useQuery } from "urql";
import { useParams } from "react-router";
import { AlbumPanel } from "@/features/album/AlbumPanel.tsx";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";

export const albumQuery = graphql(`
  query AlbumQuery($releaseGroupId: ID!) {
    releaseGroup {
      byId(id: $releaseGroupId) {
        id
        ...AlbumPanel_ReleaseGroup
      }
    }
  }
`);

export const AlbumPage = () => {
  const { releaseGroupId } = useParams<{ releaseGroupId: string }>();
  const [{ error, data, fetching, stale }] = useQuery({
    query: albumQuery,
    variables: { releaseGroupId: releaseGroupId! },
    pause: !releaseGroupId,
  });

  console.log({releaseGroupId});
  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.releaseGroup.byId) return <div>No data</div>;

  return <AlbumPanel releaseGroup={data.releaseGroup.byId} />;
};
