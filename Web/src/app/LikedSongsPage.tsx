import { LikedSongsList } from "@/features/liked-songs/LikedSongsList.tsx";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { PageLoading, PageError, PageNoData } from "@/components/ui";
import { Heart, AlertTriangle, Music } from "lucide-react";

const likedSongsQuery = graphql(`
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
  
  if (fetching || stale) return <PageLoading 
    title="Loading Liked Songs" 
    subtitle="Fetching your favorite tracks"
    icon={Heart}
    iconBgColor="bg-pink-500/20"
  />;
  
  if (error) return <PageError 
    title="Failed to Load Liked Songs" 
    message="We couldn't load your liked songs"
    error={error}
    icon={AlertTriangle}
    iconBgColor="bg-red-500/20"
  />;
  
  if (!data?.viewer) return <PageNoData 
    title="No User Data" 
    message="Your user data couldn't be loaded"
    icon={Music}
    iconBgColor="bg-yellow-500/20"
  />;
  
  return <LikedSongsList user={data?.viewer} />;
};
