import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { GradientContent } from "@/components/page-body/GradientContent.tsx";
import { AddArtistToLibraryBox } from "@/features/artist/artist-not-in-library/AddArtistToLibraryBox.tsx";
import { ArtistNotInLibraryTopTracks } from "@/features/artist/artist-not-in-library/ArtistNotInLibraryTopTracks.tsx";
import { ArtistNotInLibrarySectionList } from "@/features/artist/artist-not-in-library/ArtistNotInLibrarySectionList.tsx";
import { MainPadding } from "@/components/layout/MainPadding.tsx";

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
    <GradientContent>
      <ArtistHeader
        artistName={artist.name}
        artistBackgroundUrl={artist.images?.artistBackground ?? ""}
        listeners={artist.listeners}
        availableNumReleases={0}
      />
      <MainPadding>
        <ArtistNotInLibrarySectionList
          artistName={artist.name}
          renderTopTracks={() =>
            artist.lastFmArtist && (
              <ArtistNotInLibraryTopTracks lastFmArtist={artist.lastFmArtist} />
            )
          }
          renderImportBox={() => (
            <AddArtistToLibraryBox
              artistId={artist.id}
              artistName={artist.name}
              isInLibrary={false}
            />
          )}
        />
      </MainPadding>
    </GradientContent>
  );
};
