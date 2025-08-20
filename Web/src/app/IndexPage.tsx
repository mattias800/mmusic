import { TopArtistRecommendations } from "@/features/recommendations/top-artists/TopArtistRecommendations.tsx";
import { TopTrackRecommendations } from "@/features/recommendations/top-tracks/TopTrackRecommendations.tsx";
import { SidebarDocument } from "@/gql/graphql.ts";
import { useQuery } from "urql";
import {
  PageLayout,
  PageHeader,
  GlassCard,
  InfoSection,
  PageLoading,
  PageError,
  PageNoData,
} from "@/components/ui";
import { Music, Users, TrendingUp, Star, AlertTriangle } from "lucide-react";

const indexPageQuery = SidebarDocument;

export const IndexPage = () => {
  const [{ data, fetching, error }] = useQuery({
    query: indexPageQuery,
  });

  if (fetching)
    return (
      <PageLoading
        title="Loading Dashboard"
        subtitle="Preparing your music library overview and recommendations"
        icon={Music}
        iconBgColor="bg-blue-500/20"
      />
    );

  if (error)
    return (
      <PageError
        title="Failed to Load Dashboard"
        message="We couldn't load your music library dashboard"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );

  if (!data?.viewer)
    return (
      <PageNoData
        title="No Data Available"
        message="Your dashboard couldn't be loaded. This might be a temporary issue."
        icon={Music}
        iconBgColor="bg-yellow-500/20"
      />
    );

  return (
    <PageLayout>
      {/* Header Section */}
      <PageHeader
        icon={Music}
        title="Music Library Dashboard"
        subtitle="Discover your music collection and get personalized recommendations"
      />

      {/* Server Library Statistics moved to Admin Overview */}

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
        This dashboard provides an overview of your music collection and
        personalized recommendations based on your listening habits. The top
        artists and tracks are curated using data from Last.fm to help you
        discover new music and rediscover favorites. Your library statistics
        show the current state of your collection and help track your music
        organization progress.
      </InfoSection>
    </PageLayout>
  );
};
