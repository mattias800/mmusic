import * as React from "react";
import { TopArtistTracks } from "@/features/artist/artist-page/TopArtistTracks.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { GradientContent } from "@/components/page-body/GradientContent.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { ArtistServerStatus } from "@/features/artist/artist-server-status/ArtistServerStatus.tsx";
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
    serverStatus {
      id
      result {
        __typename
        ... on ArtistServerStatusResultBase {
          topTracksVisible
          releasesVisible
        }
        ... on ArtistServerStatusImportingArtistReleases {
          numReleaseGroupsFinishedImporting
          totalNumReleaseGroupsBeingImported
        }
      }
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

  const { releasesVisible, topTracksVisible } = artist.serverStatus.result;

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
        renderServerStatus={() => <ArtistServerStatus artistId={artist.id} />}
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
