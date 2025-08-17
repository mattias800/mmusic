import * as React from "react";
import { useMemo, useState } from "react";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { GradientButton } from "@/components/ui/gradient-button.tsx";
import { ChevronDown, RefreshCcw, UserPlus2 } from "lucide-react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { useNavigate } from "react-router";
import { getRouteToArtist, getRouteToMbArtist } from "@/AppRoutes.ts";

interface SimilarArtistsTabContentProps {
  similarArtists: Array<
    FragmentType<typeof similarArtistsTabContentSimilarArtistFragment>
  >;
  artistId?: string;
}

const similarArtistsTabContentSimilarArtistFragment = graphql(`
  fragment SimilarArtistsTabContent_SimilarArtist on SimilarArtist {
    name
    thumb
    similarityScore
    artist {
      id
    }
    musicBrainzArtistId
  }
`);

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
            musicBrainzArtistId
          }
        }
      }
      ... on RefreshArtistMetaDataError {
        message
      }
    }
  }
`);

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

export const SimilarArtistsTabContent: React.FC<
  SimilarArtistsTabContentProps
> = ({ artistId, ...props }) => {
  const similarArtist = useFragment(
    similarArtistsTabContentSimilarArtistFragment,
    props.similarArtists,
  );

  const [{ fetching }, refresh] = useMutation(refreshSimilarArtistsMutation);

  const [{ fetching: importing }, importSimilar] = useMutation(
    importSimilarMutation,
  );

  const [showAll, setShowAll] = useState(false);

  const visibleItems = useMemo(
    () => (showAll ? similarArtist : similarArtist.slice(0, 25)),
    [similarArtist, showAll],
  );

  const navigate = useNavigate();

  const maxScore = React.useMemo(() => {
    const max = Math.max(
      0,
      ...similarArtist.map((i) => i.similarityScore ?? 0),
    );
    return max > 0 ? max : 1;
  }, [similarArtist]);

  const scaledPercent = (s?: number | null) => {
    const v = Math.max(0, Math.min(1, s ?? 0));
    const relative = v / maxScore;
    return Math.round(relative * 95); // cap at 95%
  };

  if (!similarArtist || similarArtist?.length === 0) {
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

          return (
            <PhotoCard
              key={`${s.name}-${idx}`}
              imageUrl={s.thumb ?? ""}
              imageAlt={s.name}
              onClick={() => {
                if (s.artist) {
                  navigate(getRouteToArtist(s.artist.id));
                } else {
                  navigate(getRouteToMbArtist(s.musicBrainzArtistId));
                }
              }}
            >
              <PhotoCardCenterHeading>{s.name}</PhotoCardCenterHeading>
              {bar}
            </PhotoCard>
          );
        })}
      </CardFlexList>

      {similarArtist.length > 25 && !showAll && (
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
