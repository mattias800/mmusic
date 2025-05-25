import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { MessageBox } from "@/components/errors/MessageBox.tsx";
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
      topTracks {
        id
        name
        summary
        playCount
      }
      topTags {
        name
        url
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
      {data.recommendations.topArtists.map(
        (lastFmArtist) =>
          lastFmArtist.artist && (
            <ArtistCard artist={lastFmArtist.artist} key={lastFmArtist.id} />
          ),
      )}
    </CardFlexList>
  );
};
