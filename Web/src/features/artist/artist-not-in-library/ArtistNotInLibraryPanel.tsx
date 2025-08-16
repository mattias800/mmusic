import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { GlassCard, InfoSection, PageLayout } from "@/components/ui";
import { AddArtistToLibraryBox } from "@/features/artist/artist-not-in-library/AddArtistToLibraryBox.tsx";
import { ArtistNotInLibraryTopTracks } from "@/features/artist/artist-not-in-library/ArtistNotInLibraryTopTracks.tsx";
import { AlertTriangle, Music, Users } from "lucide-react";

export interface ArtistNotInLibraryPanelProps {
  artist: FragmentType<typeof artistNotInLibraryPanelMbArtistFragment>;
}

const artistNotInLibraryPanelMbArtistFragment = graphql(`
  fragment ArtistNotInLibraryPanel_MbArtist on MbArtist {
    id
    name
    images {
      artistBackground
    }
    listeners
    lastFmArtist {
      id
      ...ArtistNotInLibraryTopTracks_LastFmArtist
    }
  }
`);

export const ArtistNotInLibraryPanel: React.FC<ArtistNotInLibraryPanelProps> = (
  props,
) => {
  const artist = useFragment(
    artistNotInLibraryPanelMbArtistFragment,
    props.artist,
  );

  return (
    <>
      <ArtistHeader
        artistName={artist.name}
        artistBackgroundUrl={artist.images?.artistBackground ?? ""}
        listeners={artist.listeners}
        albumCount={0}
        epCount={0}
        singleCount={0}
      />
      <PageLayout>
        {/* Main Content */}
        <div className="max-w-7xl mx-auto space-y-8">
          {/* Artist Not in Library Info */}
          <GlassCard
            title="Artist Not in Library"
            icon={AlertTriangle}
            iconBgColor="bg-orange-500/20"
          >
            <div className="space-y-6">
              <div className="flex justify-center">
                <AddArtistToLibraryBox
                  artistId={artist.id}
                  artistName={artist.name}
                  isInLibrary={false}
                />
              </div>
            </div>
          </GlassCard>

          {/* Top Tracks Section */}
          {artist.lastFmArtist && (
            <GlassCard
              title="Popular Tracks"
              icon={Music}
              iconBgColor="bg-purple-500/20"
            >
              <div className="space-y-4">
                <div className="text-center mb-6">
                  <div className="inline-flex items-center justify-center w-12 h-12 bg-gradient-to-br from-purple-500/20 to-pink-500/20 rounded-xl border border-purple-500/30 mb-3">
                    <Music className="w-6 h-6 text-purple-400" />
                  </div>
                  <p className="text-gray-300">
                    Top tracks by{" "}
                    <span className="font-semibold text-white">
                      {artist.name}
                    </span>{" "}
                    based on Last.fm data
                  </p>
                </div>

                <ArtistNotInLibraryTopTracks
                  lastFmArtist={artist.lastFmArtist}
                />
              </div>
            </GlassCard>
          )}

          {/* Information Section */}
          <InfoSection icon={Users} title="About This Artist" variant="blue">
            This artist page shows information from external sources like
            Last.fm and MusicBrainz. To add them to your personal music library,
            use the import button above. Once imported, you'll have access to
            their full discography, local audio files, and enhanced metadata.
          </InfoSection>
        </div>
      </PageLayout>
    </>
  );
};
