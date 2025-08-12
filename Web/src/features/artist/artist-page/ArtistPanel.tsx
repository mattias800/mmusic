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
import { ArtistServiceConnections } from "@/features/artist/artist-page/ArtistServiceConnections.tsx";
import { ArtistNumReleasesAvailableIndicator } from "@/features/artist/artist-page/ArtistNumReleasesAvailableIndicator.tsx";
import { ArtistDownloadAllReleasesButton } from "@/features/artist/artist-page/ArtistDownloadAllReleasesButton.tsx";
import { ArtistImportStatusInfo } from "@/features/artist/artist-page/ArtistImportStatusInfo.tsx";

interface ArtistPanelProps {
  artist: FragmentType<typeof artistPanelArtistFragment>;
}

const artistPanelArtistFragment = graphql(`
  fragment ArtistPanel_Artist on Artist {
    id
    ...ArtistImportStatusInfo_Artist
    ...ArtistNumReleasesAvailableIndicator_Artist
    ...ArtistServiceConnections_Artist
    ...ArtistDownloadAllReleasesButton_Artist
    name
    listeners
    connectedExternalServices {
      isConnected
      externalService {
        id
        name
      }
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

  const [{ fetching: loadingTopTracks }, refreshTopTracks] = useMutation(
    refreshTopTracksMutation,
  );
  const [{ fetching: loadingLastFm }, refreshArtistMetaData] = useMutation(
    refreshArtistMetaDataMutation,
  );

  const onRefreshTopTracks = () =>
    refreshTopTracks({ input: { artistId: artist.id } });

  const onRefreshMetaData = () =>
    refreshArtistMetaData({ artistId: artist.id });

  return (
    <GradientContent>
      <ArtistHeader
        artistName={artist.name}
        artistBackgroundUrl={artist.images?.backgrounds?.[0] ?? ""}
        listeners={artist.listeners}
        renderConnections={() => <ArtistServiceConnections artist={artist} />}
      />

      <div
        className={"flex justify-between items-center gap-4 px-6 md:px-10 py-6"}
      >
        <ArtistActionButtons
          artistId={artist.id}
          artistName={artist.name}
          loadingTopTracks={loadingTopTracks}
          loadingMetaData={loadingLastFm}
          onRefreshTopTracks={onRefreshTopTracks}
          onRefreshMetaData={onRefreshMetaData}
        />

        <ArtistImportStatusInfo
          artist={artist}
          renderWhenNoInfo={() => (
            <ArtistNumReleasesAvailableIndicator
              artist={artist}
              renderDownloadAllReleasesButton={() => (
                <ArtistDownloadAllReleasesButton artist={artist} />
              )}
            />
          )}
        />
      </div>

      <MainPadding>
        <SectionList>
          <TopArtistTracks
            artistId={artist.id}
            loadingTopTracks={loadingTopTracks}
          />

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
        </SectionList>
      </MainPadding>
    </GradientContent>
  );
};
