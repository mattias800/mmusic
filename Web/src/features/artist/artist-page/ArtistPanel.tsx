import * as React from "react";
import { TopArtistTracks } from "@/features/artist/artist-page/TopArtistTracks.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { ArtistTabs, GlassCard, PageLayout } from "@/components/ui";
import { ArtistActionButtons } from "@/features/artist/artist-page/ArtistActionButtons.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { byStringField } from "@/common/sorting/Comparators.ts";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";
import { useMutation, useSubscription } from "urql";
import { ArtistServiceConnections } from "@/features/artist/artist-page/ArtistServiceConnections.tsx";
import { ArtistNumReleasesAvailableIndicator } from "@/features/artist/artist-page/ArtistNumReleasesAvailableIndicator.tsx";
import { ArtistDownloadAllReleasesButton } from "@/features/artist/artist-page/ArtistDownloadAllReleasesButton.tsx";
import { ArtistImportStatusInfo } from "@/features/artist/artist-page/ArtistImportStatusInfo.tsx";
import { ArtistStatisticsHeader } from "@/features/artist/artist-page/ArtistStatisticsHeader.tsx";
import { BarChart3, Disc3, Music, TrendingUp } from "lucide-react";
import { ArtistAppearsOnTabContent } from "@/features/artist/artist-page/ArtistAppearsOnTabContent.tsx";
import { SimilarArtistsTabContent } from "@/features/artist/artist-page/SimilarArtistsTabContent.tsx";

interface ArtistPanelProps {
  artist: FragmentType<typeof artistPanelArtistFragment>;
}

const artistPanelArtistFragment = graphql(`
  fragment ArtistPanel_Artist on Artist {
    id
    ...ArtistActionButtons_Artist
    ...ArtistImportStatusInfo_Artist
    ...ArtistNumReleasesAvailableIndicator_Artist
    ...ArtistServiceConnections_Artist
    ...ArtistDownloadAllReleasesButton_Artist
    ...ArtistStatisticsHeader_Artist
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
    alsoAppearsOn {
      coverArtUrl
      firstReleaseDate
      firstReleaseYear
      musicBrainzReleaseGroupId
      primaryArtistMusicBrainzId
      primaryArtistName
      releaseTitle
      releaseType
      role
    }
    similarArtists {
      name
      thumb
      similarityScore
      artist { id }
      musicBrainzArtistId
    }
  }
`);

const libraryArtistUpdatedSubscription = graphql(`
  subscription ArtistPanel_LibraryArtistUpdated($artistId: String!) {
    libraryArtistUpdated(artistId: $artistId) {
      id
      ...ArtistPanel_Artist
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

const refreshAllReleasesMutation = graphql(`
  mutation RefreshAllReleasesForArtist($artistId: String!) {
    refreshAllReleasesForArtist(artistId: $artistId) {
      __typename
      ... on RefreshAllReleasesForArtistSuccess {
        artistId
        refreshedCount
      }
      ... on RefreshAllReleasesForArtistError {
        message
      }
    }
  }
`);

const generateArtistShareFilesMutation = graphql(`
  mutation GenerateArtistShareFiles($input: GenerateArtistShareFilesInput!) {
    generateArtistShareFiles(input: $input) {
      __typename
      ... on GenerateArtistShareFilesSuccess {
        artistId
        artistName
        tagFileName
        manifestPath
      }
      ... on GenerateArtistShareFilesError {
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
  const [{ fetching: loadingRefreshAll }, refreshAllReleases] = useMutation(
    refreshAllReleasesMutation,
  );
  const [{ fetching: generatingShare }, generateShareFiles] = useMutation(
    generateArtistShareFilesMutation,
  );

  // Live updates: merge updated artist into cache when subscription fires
  useSubscription(
    {
      query: libraryArtistUpdatedSubscription,
      variables: { artistId: artist.id },
      pause: !artist.id,
    },
    (_prev, data) => data,
  );

  const onRefreshTopTracks = () =>
    refreshTopTracks({ input: { artistId: artist.id } });

  const onRefreshMetaData = () =>
    refreshArtistMetaData({ artistId: artist.id });

  const onRefreshAllReleases = () =>
    refreshAllReleases({ artistId: artist.id });

  const onGenerateShareFiles = () =>
    generateShareFiles({ input: { artistId: artist.id } });

  return (
    <>
      <ArtistHeader
        artistName={artist.name}
        artistBackgroundUrl={artist.images?.backgrounds?.[0] ?? ""}
        listeners={artist.listeners}
        renderConnections={() => <ArtistServiceConnections artist={artist} />}
        albumCount={artist.albums?.length ?? 0}
        epCount={artist.eps?.length ?? 0}
        singleCount={artist.singles?.length ?? 0}
      />

      <PageLayout>
        <div className="space-y-6">
          <GlassCard>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-center">
              {/* Left side - Actions */}
              <div>
                <ArtistActionButtons
                  artist={artist}
                  loadingTopTracks={loadingTopTracks}
                  loadingMetaData={
                    loadingLastFm || loadingRefreshAll || generatingShare
                  }
                  onRefreshTopTracks={onRefreshTopTracks}
                  onRefreshMetaData={onRefreshMetaData}
                  onRefreshAllReleaseMetadata={onRefreshAllReleases}
                  onGenerateShareFiles={onGenerateShareFiles}
                />
              </div>

              {/* Right side - Import Status */}
              <div className="flex justify-end">
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
            </div>
          </GlassCard>

          <GlassCard>
            <ArtistTabs
              tabs={[
                {
                  id: "top-tracks",
                  label: "Top Tracks",
                  icon: TrendingUp,
                  content: (
                    <TopArtistTracks
                      artistId={artist.id}
                      loadingTopTracks={loadingTopTracks}
                    />
                  ),
                },
                {
                  id: "albums",
                  label: "Albums",
                  icon: Disc3,
                  content: (
                    <CardFlexList>
                      {artist.albums
                        .toSorted(
                          byStringField((a) => a.firstReleaseDate ?? ""),
                        )
                        .toReversed()
                        .map((release) => (
                          <AlbumCard release={release} key={release.id} />
                        ))}
                    </CardFlexList>
                  ),
                },
                ...(artist.eps.length > 0
                  ? [
                      {
                        id: "eps",
                        label: "EPs",
                        icon: Music,
                        content: (
                          <CardFlexList>
                            {artist.eps
                              .toSorted(
                                byStringField((a) => a.firstReleaseDate ?? ""),
                              )
                              .toReversed()
                              .map((release) => (
                                <AlbumCard release={release} key={release.id} />
                              ))}
                          </CardFlexList>
                        ),
                      },
                    ]
                  : []),
                ...(artist.singles.length > 0
                  ? [
                      {
                        id: "singles",
                        label: "Singles",
                        icon: Music,
                        content: (
                          <CardFlexList>
                            {artist.singles
                              .toSorted(
                                byStringField((a) => a.firstReleaseDate ?? ""),
                              )
                              .toReversed()
                              .map((release) => (
                                <AlbumCard release={release} key={release.id} />
                              ))}
                          </CardFlexList>
                        ),
                      },
                    ]
                  : []),
                {
                  id: "media-availability",
                  label: "Media Availability",
                  icon: BarChart3,
                  content: <ArtistStatisticsHeader artist={artist} />,
                },
                {
                  id: "similar-artists",
                  label: "Similar Artists",
                  icon: TrendingUp,
                  content: (
                    <SimilarArtistsTabContent
                      artistId={artist.id}
                      items={(artist.similarArtists ?? []).map((s) => ({
                        name: s.name,
                        thumb: s.thumb,
                        similarityScore: s.similarityScore,
                        artist: s.artist ? { id: s.artist.id } : null,
                      }))}
                    />
                  ),
                },
                {
                  id: "appears-on",
                  label: "Appears On",
                  icon: Music,
                  content: (
                    <ArtistAppearsOnTabContent
                      artistId={artist.id}
                      appearsOn={artist.alsoAppearsOn}
                    />
                  ),
                },
              ]}
              defaultTab="top-tracks"
            />
          </GlassCard>
        </div>
      </PageLayout>
    </>
  );
};
