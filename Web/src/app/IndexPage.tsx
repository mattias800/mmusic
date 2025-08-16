import { TopArtistRecommendations } from "@/features/recommendations/top-artists/TopArtistRecommendations.tsx";
import { TopTrackRecommendations } from "@/features/recommendations/top-tracks/TopTrackRecommendations.tsx";
import { ServerLibraryStatisticsHeader } from "@/features/server-library/ServerLibraryStatisticsHeader.tsx";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { PageLayout, PageHeader, GlassCard, StatusCard, StatusGrid, InfoSection } from "@/components/ui";
import { Music, Disc3, Users, TrendingUp, Star, Heart, Activity } from "lucide-react";

const indexPageQuery = graphql(`
  query IndexPageQuery {
    serverLibrary {
      ...ServerLibraryStatisticsHeader_ServerLibrary
    }
  }
`);

export const IndexPage = () => {
  const [{ data, fetching, error }] = useQuery({
    query: indexPageQuery,
  });

  if (fetching) return <div className="p-6">Loading...</div>;
  if (error) return <div className="p-6">Error: {error.message}</div>;
  if (!data?.serverLibrary) return <div className="p-6">No data</div>;

  return (
    <PageLayout>
      {/* Header Section */}
      <PageHeader 
        icon={Music} 
        title="Music Library Dashboard" 
        subtitle="Discover your music collection and get personalized recommendations" 
      />

      {/* Server Library Statistics */}
      <div className="mb-12">
        <ServerLibraryStatisticsHeader serverLibrary={data.serverLibrary} />
      </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-8 max-w-7xl mx-auto">
        {/* Left Column - Top Artists */}
        <div className="space-y-8">
          <GlassCard 
            title="Top Artists" 
            icon={Users} 
            iconBgColor="bg-blue-500/20"
          >
            <TopArtistRecommendations />
          </GlassCard>
        </div>

        {/* Right Column - Top Tracks */}
        <div className="space-y-8">
          <GlassCard 
            title="Top Tracks" 
            icon={TrendingUp} 
            iconBgColor="bg-green-500/20"
          >
            <TopTrackRecommendations />
          </GlassCard>
        </div>
      </div>

      {/* Bottom Info Section */}
      <InfoSection 
        icon={Star} 
        title="About Your Music Library" 
        variant="purple"
      >
        This dashboard provides an overview of your music collection and personalized recommendations based on your listening habits. 
        The top artists and tracks are curated using data from Last.fm to help you discover new music and rediscover favorites. 
        Your library statistics show the current state of your collection and help track your music organization progress.
      </InfoSection>
    </PageLayout>
  );
};
