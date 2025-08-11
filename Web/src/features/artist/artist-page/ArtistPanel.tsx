import * as React from "react";
import { TopArtistTracks } from "@/features/artist/artist-page/TopArtistTracks.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { GradientContent } from "@/components/page-body/GradientContent.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";
// removed server status widget (legacy)
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { ArtistActionButtons } from "@/features/artist/artist-page/ArtistActionButtons.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { byStringField } from "@/common/sorting/Comparators.ts";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";
import { useMutation } from "urql";

interface ArtistPanelProps {
  artist: FragmentType<typeof artistPanelArtistFragment>;
}

const artistPanelArtistFragment = graphql(`
  fragment ArtistPanel_Artist on Artist {
    id
    name
    listeners
    connectedExternalServices {
      isConnected
      externalService { id name }
    }
    albums {
      id
      firstReleaseDate
      isFullyMissing
      ...AlbumCard_Release
    }
    eps {
      id
      firstReleaseDate
      isFullyMissing
      ...AlbumCard_Release
    }
    singles {
      id
      firstReleaseDate
      isFullyMissing
      ...AlbumCard_Release
    }
    images {
      backgrounds
    }
  }
`);

const refreshTopTracksMutation = graphql(`
  mutation PanelRefreshTopTracks($input: RefreshArtistTopTracksInput!) {
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
    }
  }
`);
const refreshArtistMetaDataMutation = graphql(`
  mutation PanelRefreshArtistMetaData($artistId: String!) {
    refreshArtistMetaData(input: { artistId: $artistId }) {
      __typename
      ... on RefreshArtistMetaDataSuccess {
        artist {
          id
          ...ArtistPanel_Artist
        }
      }
      ... on RefreshArtistMetaDataError {
        message
      }
    }
  }
`);

export const ArtistPanel: React.FC<ArtistPanelProps> = (props) => {
  const artist = useFragment(artistPanelArtistFragment, props.artist);

  const releasesVisible = true;
  const topTracksVisible = true;

  const totalNumReleases =
    artist.albums.length + artist.eps.length + artist.singles.length;

  const availableCount =
    artist.albums.filter((r) => !r.isFullyMissing).length +
    artist.eps.filter((r) => !r.isFullyMissing).length +
    artist.singles.filter((r) => !r.isFullyMissing).length;

  const [{ fetching: loadingTopTracks }, refreshTopTracks] = useMutation(
    refreshTopTracksMutation,
  );
  const [{ fetching: loadingLastFm }, refreshArtistMetaData] = useMutation(
    refreshArtistMetaDataMutation,
  );

  const onRefreshTopTracks = () =>
    refreshTopTracks({ input: { artistId: artist.id } });

  const onRefreshLastFm = () => refreshArtistMetaData({ artistId: artist.id });

  return (
    <GradientContent>
      <ArtistHeader
        artistName={artist.name}
        artistBackgroundUrl={artist.images?.backgrounds?.[0] ?? ""}
        listeners={artist.listeners}
        availableNumReleases={availableCount}
        totalNumReleases={totalNumReleases}
        renderConnections={() => (
          <div className="flex gap-2 items-center">
            {artist.connectedExternalServices.map((c) => (
              <span
                key={c.externalService.id}
                className={
                  "px-2 py-1 rounded text-xs font-medium border " +
                  (c.isConnected
                    ? "bg-green-500/20 text-green-200 border-green-400/40"
                    : "bg-white/10 text-white/70 border-white/20")
                }
                title={c.externalService.name}
              >
                {c.externalService.name}
              </span>
            ))}
          </div>
        )}
      />

      <ArtistActionButtons
        artistId={artist.id}
        loadingTopTracks={loadingTopTracks}
        loadingMetaData={loadingLastFm}
        onRefreshTopTracks={onRefreshTopTracks}
        onRefreshMetaData={onRefreshLastFm}
      />

      <MainPadding>
        <SectionList>
          {topTracksVisible && (
            <TopArtistTracks
              artistId={artist.id}
              loadingTopTracks={loadingTopTracks}
            />
          )}

          {releasesVisible && (
            <>
              <Section>
                <SectionHeading>Albums</SectionHeading>
                <CardFlexList>
                  {artist.albums
                    .toSorted(byStringField((a) => a.firstReleaseDate ?? ""))
                    .toReversed()
                    .map((release) => (
                      <AlbumCard release={release} key={release.id} />
                    ))}
                </CardFlexList>
              </Section>

              {artist.eps.length > 0 && (
                <Section>
                  <SectionHeading>EPs</SectionHeading>
                  <CardFlexList>
                    {artist.eps
                      .toSorted(byStringField((a) => a.firstReleaseDate ?? ""))
                      .toReversed()
                      .map((release) => (
                        <AlbumCard release={release} key={release.id} />
                      ))}
                  </CardFlexList>
                </Section>
              )}

              {artist.singles.length > 0 && (
                <Section>
                  <SectionHeading>Singles</SectionHeading>
                  <CardFlexList>
                    {artist.singles
                      .toSorted(byStringField((a) => a.firstReleaseDate ?? ""))
                      .toReversed()
                      .map((release) => (
                        <AlbumCard release={release} key={release.id} />
                      ))}
                  </CardFlexList>
                </Section>
              )}
            </>
          )}
        </SectionList>
      </MainPadding>
    </GradientContent>
  );
};
