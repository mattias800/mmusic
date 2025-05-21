import { LikedSongsList } from "@/features/liked-songs/LikedSongsList.tsx";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";

export const likedSongsQuery = graphql(`
  query LikedSongsQuery {
    viewer {
      id
      ...LikedSongsList_User
    }
  }
`);

export const LikedSongsPage = () => {
  const [{ error, data, fetching, stale }] = useQuery({
    query: likedSongsQuery,
  });
  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.viewer) return <div>No data</div>;
  return <LikedSongsList user={data?.viewer} />;
};
