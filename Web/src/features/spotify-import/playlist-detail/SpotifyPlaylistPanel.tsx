import React from "react";
import { useQuery } from "urql";
import { GradientContent } from "@/components/page-body/GradientContent.tsx";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { Button } from "@/components/ui/button.tsx";
import { Play, Download, CheckSquare, Square } from "lucide-react";
import { PlaylistSummaryHeader } from "@/common/PlaylistSummaryHeader.tsx";

export interface SpotifyPlaylistPanelProps {
  playlistId: string;
}

const spotifyPlaylistDetailsQuery = `
  query SpotifyPlaylistDetails($playlistId: String!) {
    playlist {
      importPlaylists {
        spotify {
          byId: spotifyPlaylistById(id: $playlistId) {
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
        }
      }
    }
  }
` as any;

type TrackSelectionState = Record<string, boolean>;

export const SpotifyPlaylistPanel: React.FC<SpotifyPlaylistPanelProps> = ({
  playlistId,
}) => {
  const [{ data, fetching, error }] = useQuery<any, { playlistId: string }>({
    query: spotifyPlaylistDetailsQuery,
    variables: { playlistId },
    requestPolicy: "network-only",
  });

  const [selected, setSelected] = React.useState<TrackSelectionState>({});
  const [selectAll, setSelectAll] = React.useState(false);

  React.useEffect(() => {
    const tracks = data?.playlist?.importPlaylists?.spotify?.byId?.tracks ?? [];
    const next: TrackSelectionState = {};
    tracks.forEach((t) => (next[t.id] = selectAll));
    setSelected(next);
  }, [data?.playlist?.importPlaylists?.spotify?.byId?.id, selectAll]);

  if (fetching) return null;
  if (error)
    return <div className="p-4 text-red-400">Error: {error.message}</div>;

  const playlist = data?.playlist?.importPlaylists?.spotify?.byId;
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
                  <Button size="sm" variant="secondary" iconLeft={Download} disabled>
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
                </>
              }
            />
          </Section>

          <Section>
            <SectionHeading>Tracks</SectionHeading>
            <div className="mt-2">
              {tracks.map((t, index) => (
                <div key={t.id} className="flex items-center">
                  <input
                    type="checkbox"
                    className="mr-3 ml-4"
                    checked={!!selected[t.id]}
                    onChange={(e) =>
                      setSelected((prev) => ({ ...prev, [t.id]: e.target.checked }))
                    }
                  />
                  <TrackItem
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
                  />
                </div>
              ))}
            </div>
          </Section>
        </SectionList>
      </MainPadding>
    </GradientContent>
  );
};


