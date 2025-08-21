import React, { useEffect } from "react";
import { GradientContent } from "@/components/page-body/GradientContent.tsx";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Button } from "@/components/ui/button.tsx";
import {
  CheckSquare,
  Download,
  Library,
  Play,
  Square,
  Users,
} from "lucide-react";
import { PlaylistSummaryHeader } from "@/common/PlaylistSummaryHeader.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation, useQuery } from "urql";
import { ExternalPlaylistTrackListHeading } from "@/features/spotify-import/playlist-detail/ExternalPlaylistTrackListHeading.tsx";
import { ExternalPlaylistTrackItem } from "@/features/spotify-import/playlist-detail/ExternalPlaylistTrackItem.tsx";
import { Checkbox } from "@/components/ui/checkbox.tsx";

export interface SpotifyPlaylistPanelProps {
  playlist: FragmentType<typeof spotifyPlaylistPanelPlaylistFragment>;
}

const spotifyPlaylistPanelPlaylistFragment = graphql(`
  fragment SpotifyPlaylistPanel_SpotifyPlaylist on SpotifyPlaylist {
    id
    name
    description
    coverImageUrl
    totalTracks
    tracks {
      id
      title
      durationMs
      artistNames
      albumCoverImageUrl
      previewUrl
    }
  }
`);

const viewerQuery = graphql(`
  query ViewerIdForSpotifyImport {
    viewer {
      id
    }
  }
`);

const importSpotifyPlaylistByIdMutation = graphql(`
  mutation ImportSpotifyPlaylist($playlistId: String!, $userId: UUID!) {
    importSpotifyPlaylist(input: { playlistId: $playlistId, userId: $userId }) {
      __typename
      ... on ImportSpotifyPlaylistSuccess {
        playlist {
          id
          name
        }
      }
      ... on ImportSpotifyPlaylistError {
        message
      }
    }
  }
`);

const enqueueArtistsFromPlaylistMutation = graphql(`
  mutation EnqueueArtistsFromSpotifyPlaylist($playlistId: String!) {
    enqueueArtistsFromSpotifyPlaylist(input: { playlistId: $playlistId })
  }
`);

type TrackSelectionState = Record<string, boolean>;

export const SpotifyPlaylistPanel: React.FC<SpotifyPlaylistPanelProps> = (
  props,
) => {
  const playlist = useFragment(
    spotifyPlaylistPanelPlaylistFragment,
    props.playlist,
  );

  const [selected, setSelected] = React.useState<TrackSelectionState>({});
  const [selectAll, setSelectAll] = React.useState(false);

  useEffect(() => {
    const tracks = playlist.tracks ?? [];
    const next: TrackSelectionState = {};
    tracks.forEach((t) => (next[t.id] = selectAll));
    setSelected(next);
  }, [playlist.id, playlist.tracks, selectAll]);

  const [{ data: viewerData }] = useQuery({ query: viewerQuery });

  const [{ fetching: isImporting }, importPlaylist] = useMutation(
    importSpotifyPlaylistByIdMutation,
  );
  const [, enqueueArtists] = useMutation(enqueueArtistsFromPlaylistMutation);

  const handleImportPlaylist = async () => {
    if (!viewerData?.viewer?.id) return;
    await importPlaylist({
      playlistId: playlist.id,
      userId: viewerData.viewer.id,
    });
  };

  const handleEnqueueArtists = async () => {
    await enqueueArtists({ playlistId: playlist.id });
  };

  if (!playlist) return <div className="p-4">Playlist not found.</div>;

  const tracks = playlist.tracks ?? [];
  const totalMs = tracks.reduce((sum, t) => sum + (t.durationMs ?? 0), 0);
  const selectedCount = Object.values(selected).filter(Boolean).length;

  return (
    <GradientContent>
      <MainPadding>
        <SectionList>
          <Section>
            <PlaylistSummaryHeader
              title={playlist.name}
              description={playlist.description}
              coverImageUrl={playlist.coverImageUrl ?? undefined}
              trackCount={tracks.length}
              totalMinutes={totalMs / 60000}
              actions={
                <>
                  <div className="flex items-center gap-2">
                    <Button size="sm" iconLeft={Play} disabled>
                      Play (Spotify)
                    </Button>
                  </div>
                </>
              }
            />
          </Section>

          <Section>
            <div className="flex flex-col gap-2">
              <div className="flex items-center gap-2 flex-wrap">
                <Button
                  variant="ghost"
                  iconLeft={selectAll ? CheckSquare : Square}
                  onClick={() => setSelectAll((s) => !s)}
                >
                  {selectAll ? "Unselect all" : "Select all"}
                </Button>
                <Button
                  variant="secondary"
                  iconLeft={Download}
                  disabled
                  title="Create a local playlist from selected tracks (coming soon)"
                >
                  Create playlist with selected tracks ({selectedCount})
                </Button>
              </div>
              <div className="flex items-center gap-2 flex-wrap">
                <Button
                  variant="secondary"
                  iconLeft={Users}
                  onClick={handleEnqueueArtists}
                  title="Adds all artists from this playlist to the Import queue"
                >
                  Import all artists
                </Button>
                <Button
                  variant="secondary"
                  iconLeft={Library}
                  onClick={handleImportPlaylist}
                  loading={isImporting}
                >
                  Import playlist
                </Button>
              </div>
            </div>
          </Section>

          <Section>
            <SectionHeading>Tracks</SectionHeading>
            <div className="mt-2">
              <ExternalPlaylistTrackListHeading showCoverArt />
              {tracks.map((t, index) => (
                <ExternalPlaylistTrackItem
                  key={t.id}
                  title={t.title}
                  trackNumber={index + 1}
                  playCount={0}
                  trackLength={Math.round((t.durationMs ?? 0) / 1000)}
                  showCoverArt
                  renderCoverArt={() => (
                    <img
                      src={t.albumCoverImageUrl ?? ""}
                      alt={t.title}
                      className="h-12 w-12 rounded"
                    />
                  )}
                  renderSubtitle={() => <span>{t.artistNames.join(", ")}</span>}
                  renderCheckbox={() => (
                    <div className="mr-3 ml-4">
                      <Checkbox
                        checked={selected[t.id]}
                        onCheckedChange={(checked) =>
                          setSelected((prev) => ({ ...prev, [t.id]: checked }))
                        }
                        aria-label={`Select ${t.title}`}
                      />
                    </div>
                  )}
                />
              ))}
            </div>
          </Section>
        </SectionList>
      </MainPadding>
    </GradientContent>
  );
};
