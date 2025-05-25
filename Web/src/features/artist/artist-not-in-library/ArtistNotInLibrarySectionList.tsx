import * as React from "react";
import { ArtistInLibraryButton } from "@/features/add-artist-to-server-library/ArtistInLibraryButton.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { CircleAlert } from "lucide-react";
import { ArtistNotInLibraryTopTracks } from "@/features/artist/artist-not-in-library/ArtistNotInLibraryTopTracks.tsx";

export interface ArtistNotInLibrarySectionListProps {
  artist: FragmentType<typeof artistNotInLibraryPanelArtistFragment>;
}

export const artistNotInLibraryPanelArtistFragment = graphql(`
  fragment ArtistNotInLibraryPanel_Artist on Artist {
    id
    name
    ...ArtistHeader_Artist
    ...ArtistInLibraryButton_Artist
    ...ArtistNotInLibraryTopTracks_Artist
  }
`);

export const ArtistNotInLibrarySectionList: React.FC<
  ArtistNotInLibrarySectionListProps
> = (props) => {
  const artist = useFragment(
    artistNotInLibraryPanelArtistFragment,
    props.artist,
  );

  return (
    <SectionList>
      <Section>
        <SectionHeading>Popular tracks by {artist.name}</SectionHeading>
        <div className={"flex gap-32"}>
          <ArtistNotInLibraryTopTracks artist={artist} />
          <div
            className={
              "flex flex-col gap-4 p-4 bg-black/70 rounded-md justify-between"
            }
          >
            <div className={"flex gap-4 items-center"}>
              <CircleAlert />
              <span className={"text-2xl"}>Not in library</span>
            </div>
            <span>{artist.name} is not in your library, click to add.</span>
            <div className={"flex justify-end"}>
              <ArtistInLibraryButton artist={artist} />
            </div>
          </div>
        </div>
      </Section>
    </SectionList>
  );
};
