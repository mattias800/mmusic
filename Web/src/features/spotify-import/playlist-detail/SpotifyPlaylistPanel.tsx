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
  Play,
  Square,
  Library,
  Users,
} from "lucide-react";
import { PlaylistSummaryHeader } from "@/common/PlaylistSummaryHeader.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation, useQuery } from "urql";
import { ExternalPlaylistTrackListHeading } from "@/features/spotify-import/playlist-detail/ExternalPlaylistTrackListHeading.tsx";
import { ExternalPlaylistTrackItem } from "@/features/spotify-import/playlist-detail/ExternalPlaylistTrackItem.tsx";
import { graphql as g } from "@/gql";

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
      ... on ImportSpotifyPlaylistSuccess { playlist { id name } }
      ... on ImportSpotifyPlaylistError { message }
    }
  }
`);

const importArtistsFromSpotifyPlaylistMutation = graphql(`
  mutation ImportArtistsFromSpotifyPlaylist($playlistId: String!) {
    importArtistsFromSpotifyPlaylist(input: { playlistId: $playlistId }) {
      __typename
      ... on ImportArtistsFromSpotifyPlaylistSuccess {
        artists { id name }
        totalArtists
        importedArtists
        failedArtists
      }
      ... on ImportArtistsFromSpotifyPlaylistError { message }
    }
  }
`);

const enqueueArtistsFromPlaylistMutation = g(`
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

  const [, importPlaylist] = useMutation(importSpotifyPlaylistByIdMutation);
  const [, importArtists] = useMutation(
    importArtistsFromSpotifyPlaylistMutation,
  );
  const [, enqueueArtists] = useMutation(enqueueArtistsFromPlaylistMutation);

  const handleImportPlaylist = async () => {
    if (!viewerData?.viewer?.id) return;
    await importPlaylist({
      playlistId: playlist.id,
      userId: viewerData.viewer.id,
    });
  };

  const handleImportArtists = async () => {
    await importArtists({ playlistId: playlist.id });
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
                  <Button size="sm" iconLeft={Play} disabled>
                    Play (Spotify)
                  </Button>
                  <Button
                    size="sm"
                    variant="secondary"
                    iconLeft={Download}
                    disabled
                  >
                    Import selected ({selectedCount})
                  </Button>
                  <Button
                    size="sm"
                    variant="ghost"
                    iconLeft={selectAll ? CheckSquare : Square}
                    onClick={() => setSelectAll((s) => !s)}
                  >
                    {selectAll ? "Unselect all" : "Select all"}
                  </Button>
                  <Button
                    size="sm"
                    iconLeft={Library}
                    onClick={handleImportPlaylist}
                  >
                    Import playlist
                  </Button>
                  <Button
                    size="sm"
                    variant="secondary"
                    iconLeft={Users}
                    onClick={handleImportArtists}
                  >
                    Import artists
                  </Button>
                  <Button
                    size="sm"
                    variant="ghost"
                    iconLeft={Users}
                    onClick={handleEnqueueArtists}
                  >
                    Enqueue artists
                  </Button>
                </>
              }
            />
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
                    <input
                      type="checkbox"
                      className="mr-3 ml-4"
                      checked={selected[t.id]}
                      onChange={(e) =>
                        setSelected((prev) => ({
                          ...prev,
                          [t.id]: e.target.checked,
                        }))
                      }
                    />
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
