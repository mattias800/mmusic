import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { ErrorBox } from "@/components/errors/ErrorBox.tsx";
import { ArtistCard } from "@/features/artist/artist-card/ArtistCard.tsx";

export interface TopArtistRecommendationsProps {}

const topArtistRecommendationsQuery = graphql(`
  query TopArtistRecommendations {
    recommendations {
      topArtists {
        id
        artist {
          id
          ...ArtistCard_Artist
        }
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
    return <ErrorBox message={error?.message ?? "No data"} />;
  }

  return (
    <CardFlexList>
      {data.recommendations.topArtists
        .filter((l) => l.artist)
        .map((lastFmArtist) => (
          <ArtistCard artist={lastFmArtist.artist} key={lastFmArtist.id} />
        ))}
    </CardFlexList>
  );
};
