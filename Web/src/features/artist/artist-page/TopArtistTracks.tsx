import * as React from "react";
import { useState } from "react";
import { TopArtistTrackItem } from "@/features/artist/artist-page/TopArtistTrackItem.tsx";
import { graphql } from "@/gql";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { useQuery } from "urql";
import { TopTrackShimmer } from "@/features/artist/artist-page/TopTrackShimmer.tsx";
import { ShowMoreButton } from "@/components/buttons/ShowMoreButton.tsx";
import { useParams } from "react-router";

export interface TopArtistTracksProps {}

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

export const TopArtistTracks: React.FC<TopArtistTracksProps> = () => {
  const { artistId } = useParams<{ artistId: string }>();

  const [{ data, fetching, error }] = useQuery({
    query: topArtistTracksArtistQuery,
    variables: { artistId: artistId ?? "" },
    pause: !artistId,
  });

  const [showingMore, setShowingMore] = useState(false);

  const visibleTracks = (data?.serverLibrary.artistById?.topTracks ?? []).slice(
    0,
    showingMore ? 20 : 10,
  );

  if (fetching) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>Error: {error.message}</div>;
  }

  if (visibleTracks.length === 0) {
    return null;
  }

  return (
    <Section>
      <div className={"flex gap-4 items-center"}>
        <SectionHeading>Popular</SectionHeading>
      </div>

      {fetching ? (
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
