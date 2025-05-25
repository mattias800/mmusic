import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { MessageBox } from "@/components/errors/MessageBox.tsx";
import { TopArtistCard } from "@/features/artist/artist-card/TopArtistCard.tsx";

export interface TopArtistRecommendationsProps {}

const topArtistRecommendationsQuery = graphql(`
  query TopArtistRecommendations {
    recommendations {
      topArtists {
        id
        ...TopArtistCard_Artist
      }
    }
  }
`);

export const TopArtistRecommendations: React.FC<
  TopArtistRecommendationsProps
> = () => {
  const [{ fetching, data, error }] = useQuery({
    query: topArtistRecommendationsQuery,
  });
  if (fetching)
    return (
      <SpinnerSpacing>
        <Spinner />
      </SpinnerSpacing>
    );

  if (error || !data) {
    return (
      <MessageBox message={error?.message ?? "No data"} variant={"error"} />
    );
  }

  return (
    <CardFlexList>
      {data.recommendations.topArtists.map((lastFmArtist) => (
        <TopArtistCard artist={lastFmArtist} key={lastFmArtist.id} />
      ))}
    </CardFlexList>
  );
};
