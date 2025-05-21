import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { ErrorBox } from "@/components/errors/ErrorBox.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { TopTrackCard } from "@/features/recommendations/top-tracks/TopTrackCard.tsx";

export interface TopTrackRecommendationsProps {}

const topTrackRecommendationsQuery = graphql(`
  query TopTrackRecommendations {
    recommendations {
      topTracks {
        id
        ...TopTrackCard_LastFmTrack
      }
    }
  }
`);

export const TopTrackRecommendations: React.FC<
  TopTrackRecommendationsProps
> = () => {
  const [{ fetching, data, error }] = useQuery({
    query: topTrackRecommendationsQuery,
  });
  if (fetching)
    return (
      <SpinnerSpacing>
        <Spinner />
      </SpinnerSpacing>
    );

  if (error || !data) {
    return <ErrorBox message={error?.message ?? "No data"} />;
  }

  return (
    <CardFlexList>
      {data.recommendations.topTracks.map((lastFmTrack) => (
        <TopTrackCard track={lastFmTrack} key={lastFmTrack.id} />
      ))}
    </CardFlexList>
  );
};
