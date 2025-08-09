import * as React from "react";
import { useState } from "react";
import { TopArtistTrackItem } from "@/features/artist/artist-page/TopArtistTrackItem.tsx";
import { graphql } from "@/gql";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { useMutation, useQuery } from "urql";
import { TopTrackShimmer } from "@/features/artist/artist-page/TopTrackShimmer.tsx";
import { ShowMoreButton } from "@/components/buttons/ShowMoreButton.tsx";

export interface TopArtistTracksProps {
  artistId: string;
}

const topArtistTracksArtistQuery = graphql(`
  query TopArtistTracks($artistId: ID!) {
    serverLibrary {
      artistById(id: $artistId) {
        id
        topTracks {
          ...TopArtistTrackItem_ArtistTopTrack
        }
      }
    }
  }
`);

const refreshTopTracksMutation = graphql(`
  mutation RefreshArtistTopTracks($input: RefreshArtistTopTracksInput!) {
    refreshArtistTopTracks(input: $input) {
      __typename
      ... on RefreshArtistTopTracksSuccess {
        artist {
          id
          topTracks {
            ...TopArtistTrackItem_ArtistTopTrack
          }
        }
      }
      ... on RefreshArtistTopTracksUnknownError {
        message
      }
    }
  }
`);

export const TopArtistTracks: React.FC<TopArtistTracksProps> = ({
  artistId,
}) => {
  const [{ data, fetching }] = useQuery({
    query: topArtistTracksArtistQuery,
    variables: { artistId },
  });
  const [{ fetching: refreshing }] = useMutation(refreshTopTracksMutation);

  const [showingMore, setShowingMore] = useState(false);

  const visibleTracks = (data?.serverLibrary.artistById?.topTracks ?? []).slice(
    0,
    showingMore ? 20 : 10,
  );

  if (visibleTracks.length === 0) {
    return null;
  }

  return (
    <Section>
      <div className={"flex gap-4 items-center"}>
        <SectionHeading loading={fetching || refreshing}>
          Popular
        </SectionHeading>
      </div>

      {fetching || refreshing ? (
        <div>
          <TopTrackShimmer trackNumber={1} />
          <TopTrackShimmer trackNumber={2} />
          <TopTrackShimmer trackNumber={3} />
          <TopTrackShimmer trackNumber={4} />
          <TopTrackShimmer trackNumber={5} />
          <TopTrackShimmer trackNumber={6} />
          <TopTrackShimmer trackNumber={7} />
          <TopTrackShimmer trackNumber={8} />
          <TopTrackShimmer trackNumber={9} />
          <TopTrackShimmer trackNumber={10} />
        </div>
      ) : (
        <>
          <div>
            {visibleTracks.map((track, index) => (
              <TopArtistTrackItem
                key={index}
                index={index + 1}
                artistTopTrack={track}
              />
            ))}
          </div>
          {!showingMore && (
            <div className={"pl-14 pt-8"}>
              <ShowMoreButton onClick={() => setShowingMore(true)} />
            </div>
          )}
        </>
      )}
    </Section>
  );
};
