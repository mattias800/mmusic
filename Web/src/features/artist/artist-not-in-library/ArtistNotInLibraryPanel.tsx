import * as React from "react";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { GradientContent } from "@/components/page-body/GradientContent.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistNotInLibrarySectionList } from "@/features/artist/artist-not-in-library/ArtistNotInLibrarySectionList.tsx";

export interface ArtistNotInLibraryPanelProps {
  artist: FragmentType<typeof artistNotInLibraryPanelMbArtistFragment>;
}

export const artistNotInLibraryPanelMbArtistFragment = graphql(`
  fragment ArtistNotInLibraryPanel_MbArtist on MbArtist {
    id
    name
    listeners
    images {
      artistBackground
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
      />
      <ArtistNotInLibrarySectionList
        artistId={artist.id}
        artistName={artist.name}
        isInLibrary={false}
      />
    </GradientContent>
  );
};
