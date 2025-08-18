import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { GlassCard, PageLayout, TabMenu } from "@/components/ui";
import { ArtistActionButtons } from "@/features/artist/artist-page/ArtistActionButtons.tsx";
import { useMutation, useSubscription } from "urql";
import { ArtistServiceConnections } from "@/features/artist/artist-page/ArtistServiceConnections.tsx";
import { ArtistNumReleasesAvailableIndicator } from "@/features/artist/artist-page/ArtistNumReleasesAvailableIndicator.tsx";
import { ArtistDownloadAllReleasesButton } from "@/features/artist/artist-page/ArtistDownloadAllReleasesButton.tsx";
import { ArtistImportStatusInfo } from "@/features/artist/artist-page/ArtistImportStatusInfo.tsx";
import { BarChart3, Disc3, Music, TrendingUp } from "lucide-react";
import { Outlet, useLocation, useNavigate } from "react-router";
import { TabItem } from "@/components/ui/TabItem.tsx";

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
    }
    eps {
      id
    }
    singles {
      id
    }
    images {
      backgrounds
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

  const navigate = useNavigate();
  const location = useLocation();

  const tab = location.pathname.split("/")[3] ?? "";

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
  useSubscription({
    query: libraryArtistUpdatedSubscription,
    variables: { artistId: artist.id },
    pause: !artist.id,
  });

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
            <TabMenu>
              <TabItem
                label={"Top Tracks"}
                icon={TrendingUp}
                onClick={() =>
                  navigate(`/artist/${artist.id}`, { replace: true })
                }
                isActive={tab === "top-tracks"}
                isActive={tab === "top-tracks" || !tab}
              />

              {artist.albums.length > 0 && (
                <TabItem
                  label={"Albums"}
                  icon={Disc3}
                  onClick={() =>
                    navigate(`/artist/${artist.id}/albums`, { replace: true })
                  }
                  isActive={tab === "albums"}
                />
              )}
              {artist.eps.length > 0 && (
                <TabItem
                  label={"EPs"}
                  icon={Music}
                  onClick={() =>
                    navigate(`/artist/${artist.id}/eps`, { replace: true })
                  }
                  isActive={tab === "eps"}
                />
              )}
              {artist.singles.length > 0 && (
                <TabItem
                  label={"Singles"}
                  icon={Music}
                  onClick={() =>
                    navigate(`/artist/${artist.id}/singles`, { replace: true })
                  }
                  isActive={tab === "singles"}
                />
              )}

              <TabItem
                label={"Similar Artists"}
                icon={TrendingUp}
                onClick={() =>
                  navigate(`/artist/${artist.id}/similar-artists`, {
                    replace: true,
                  })
                }
                isActive={tab === "similar-artists"}
              />

              <TabItem
                label={"Appears On"}
                icon={Music}
                onClick={() =>
                  navigate(`/artist/${artist.id}/appears-on`, { replace: true })
                }
                isActive={tab === "appears-on"}
              />

              <TabItem
                label={"Media Availability"}
                icon={BarChart3}
                onClick={() =>
                  navigate(`/artist/${artist.id}/media-availability`)
                }
                isActive={tab === "media-availability"}
              />
            </TabMenu>
            <div className="min-h-[200px] pt-6">
              <Outlet />
            </div>
          </GlassCard>
        </div>
      </PageLayout>
    </>
  );
};
