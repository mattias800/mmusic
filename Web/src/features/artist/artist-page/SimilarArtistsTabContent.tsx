import * as React from "react";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { GradientButton } from "@/components/ui/gradient-button.tsx";
import { ChevronDown, RefreshCcw, UserPlus2 } from "lucide-react";
import { graphql } from "@/gql";
import { useMutation } from "urql";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { useNavigate } from "react-router";
import { getRouteToArtist } from "@/AppRoutes.ts";
// import { ArtistCard } from "@/features/artist/artist-card/ArtistCard.tsx";
type SimilarArtistsTabItem = {
  name: string;
  thumb?: string | null;
  similarityScore?: number | null;
  artist?: { id: string } | null;
};

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
          artist {
            id
          }
        }
        ... on ImportSimilarArtistsError {
          message
        }
      }
    }
  `);
  const [{ fetching: importing }, importSimilar] = useMutation(
    importSimilarMutation,
  );
  const [showAll, setShowAll] = React.useState(false);
  const visibleItems = React.useMemo(
    () => (showAll ? items : items.slice(0, 25)),
    [items, showAll],
  );
  const navigate = useNavigate();

  const maxScore = React.useMemo(() => {
    const max = Math.max(0, ...items.map((i) => i.similarityScore ?? 0));
    return max > 0 ? max : 1;
  }, [items]);
  const scaledPercent = (s?: number | null) => {
    const v = Math.max(0, Math.min(1, s ?? 0));
    const relative = v / maxScore;
    return Math.round(relative * 95); // cap at 95%
  };

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
      </div>

      <CardFlexList>
        {visibleItems.map((s, idx) => {
          const bar = (
            <div className="absolute bottom-2 left-2 right-2 z-10">
              <div className="h-1.5 w-full rounded-full bg-white/20 overflow-hidden">
                <div
                  className="h-1.5 rounded-full bg-gradient-to-r from-emerald-400 to-blue-500"
                  style={{ width: `${scaledPercent(s.similarityScore)}%` }}
                />
              </div>
            </div>
          );

          if (s.artist) {
            return (
              <PhotoCard
                key={s.artist.id}
                imageUrl={s.thumb ?? ""}
                imageAlt={s.name}
                onClick={() => navigate(getRouteToArtist(s.artist!.id))}
              >
                <PhotoCardCenterHeading>{s.name}</PhotoCardCenterHeading>
                {bar}
              </PhotoCard>
            );
          }
          return (
            <PhotoCard
              key={`${s.name}-${idx}`}
              imageUrl={s.thumb ?? ""}
              imageAlt={s.name}
              onClick={() =>
                window.open(
                  `https://musicbrainz.org/search?query=${encodeURIComponent(s.name)}&type=artist&method=indexed`,
                  "_blank",
                )
              }
            >
              <PhotoCardCenterHeading>{s.name}</PhotoCardCenterHeading>
              {bar}
            </PhotoCard>
          );
        })}
      </CardFlexList>

      {items.length > 25 && !showAll && (
        <div className="flex justify-center">
          <GradientButton
            onClick={() => setShowAll(true)}
            iconLeft={ChevronDown}
            variant="secondary"
            size="sm"
          >
            Show more
          </GradientButton>
        </div>
      )}
    </div>
  );
};
