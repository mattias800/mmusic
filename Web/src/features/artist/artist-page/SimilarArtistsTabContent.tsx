import * as React from "react";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { GradientButton } from "@/components/ui/gradient-button.tsx";
import { RefreshCcw, UserPlus2, ChevronDown, ChevronUp } from "lucide-react";
import { graphql } from "@/gql";
import { useMutation } from "urql";
// import { ArtistCard } from "@/features/artist/artist-card/ArtistCard.tsx";
type SimilarArtistsTabItem = {
  name: string;
  thumb?: string | null;
  similarityScore?: number | null;
  artist?: { id: string } | null;
};
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { PhotoCardBottomText } from "@/components/cards/PhotoCardBottomText.tsx";

interface SimilarArtistsTabContentProps {
  items: SimilarArtistsTabItem[];
  artistId?: string;
}

const refreshSimilarArtistsMutation = graphql(`
  mutation RefreshSimilarArtists($artistId: String!) {
    refreshArtistMetaData(input: { artistId: $artistId }) {
      __typename
      ... on RefreshArtistMetaDataSuccess {
        artist {
          id
          similarArtists {
            name
            thumb
            similarityScore
            artist {
              id
            }
          }
        }
      }
      ... on RefreshArtistMetaDataError {
        message
      }
    }
  }
`);

export const SimilarArtistsTabContent: React.FC<
  SimilarArtistsTabContentProps
> = ({ items, artistId }) => {
  const [{ fetching }, refresh] = useMutation(refreshSimilarArtistsMutation);
  const importSimilarMutation = graphql(`
    mutation ImportSimilarArtists($artistId: String!) {
      importSimilarArtists(input: { artistId: $artistId }) {
        __typename
        ... on ImportSimilarArtistsSuccess {
          importedCount
          artist { id }
        }
        ... on ImportSimilarArtistsError { message }
      }
    }
  `);
  const [{ fetching: importing }, importSimilar] = useMutation(importSimilarMutation);
  const [showAll, setShowAll] = React.useState(false);
  const visibleItems = React.useMemo(() => (showAll ? items : items.slice(0, 25)), [items, showAll]);
  if (!items || items.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center text-center p-8 gap-8">
        <div className="w-16 h-16 rounded-full bg-gradient-to-br from-purple-500/30 to-blue-500/30 flex items-center justify-center">
          <span className="text-2xl">✨</span>
        </div>
        <div>
          <p className="text-white font-semibold text-lg">
            No similar artists yet
          </p>
          <p className="text-gray-400 text-sm mt-1">
            Try refreshing to fetch recommendations based on community
            listening.
          </p>
        </div>
        {artistId && (
          <GradientButton
            onClick={() => refresh({ artistId })}
            loading={fetching}
            disabled={fetching}
            iconLeft={RefreshCcw}
          >
            Refresh similar artists
          </GradientButton>
        )}
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          {artistId && (
            <GradientButton
              onClick={() => refresh({ artistId })}
              loading={fetching}
              disabled={fetching}
              iconLeft={RefreshCcw}
              variant="secondary"
              size="sm"
            >
              Refresh
            </GradientButton>
          )}
          {artistId && (
            <GradientButton
              onClick={() => importSimilar({ artistId })}
              loading={importing}
              disabled={importing}
              iconLeft={UserPlus2}
              size="sm"
            >
              Import all similar artists
            </GradientButton>
          )}
        </div>
        {items.length > 25 && (
          <GradientButton
            onClick={() => setShowAll((v) => !v)}
            iconLeft={showAll ? ChevronUp : ChevronDown}
            variant="secondary"
            size="sm"
          >
            {showAll ? "Show less" : `Show more (${items.length - 25})`}
          </GradientButton>
        )}
      </div>

      <CardFlexList>
      {visibleItems.map((s, idx) => {
        if (s.artist) {
          // Local artist - render full ArtistCard
          // We only fetched id; ArtistCard expects its full fragment
          // For now, render fallback card for remote artists without full data
          // Until we add a lightweight ArtistCard variant or fetch the full fragment here
          return (
            <PhotoCard
              key={s.artist.id}
              imageUrl={s.thumb ?? ""}
              imageAlt={s.name}
            >
              <PhotoCardCenterHeading>{s.name}</PhotoCardCenterHeading>
              {s.similarityScore != null && (
                <PhotoCardBottomText>{`${(s.similarityScore * 100).toFixed(0)}% match`}</PhotoCardBottomText>
              )}
            </PhotoCard>
          );
        }
        // Fallback simple card with stored thumb and name
        return (
          <PhotoCard
            key={`${s.name}-${idx}`}
            imageUrl={s.thumb ?? ""}
            imageAlt={s.name}
          >
            <PhotoCardCenterHeading>{s.name}</PhotoCardCenterHeading>
            {s.similarityScore != null && (
              <PhotoCardBottomText>{`${(s.similarityScore * 100).toFixed(0)}% match`}</PhotoCardBottomText>
            )}
          </PhotoCard>
        );
      })}
      </CardFlexList>
    </div>
  );
};
