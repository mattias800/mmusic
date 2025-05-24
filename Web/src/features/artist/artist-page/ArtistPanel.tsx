import * as React from "react";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";
import { TopArtistTracks } from "@/features/artist/artist-page/TopArtistTracks.tsx";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import { FollowButton } from "@/components/buttons/FollowButton.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistAlbumList } from "@/features/artist/artist-page/ArtistAlbumList.tsx";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { ArtistInLibraryButton } from "@/features/add-artist-to-server-library/ArtistInLibraryButton.tsx";
import { ArtistSingleList } from "@/features/artist/artist-page/ArtistSingleList.tsx";
import { GradientContent } from "@/components/page-body/GradientContent.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { ArtistEpList } from "@/features/artist/artist-page/ArtistEpList.tsx";

interface ArtistPanelProps {
  artist: FragmentType<typeof artistPanelArtistFragment>;
}

export const artistPanelArtistFragment = graphql(`
  fragment ArtistPanel_Artist on Artist {
    id
    name
    ...ArtistHeader_Artist
    ...TopArtistTracks_Artist
    ...ArtistInLibraryButton_Artist
  }
`);

export const ArtistPanel: React.FC<ArtistPanelProps> = (props) => {
  const artist = useFragment(artistPanelArtistFragment, props.artist);

  return (
    <GradientContent>
      <ArtistHeader artist={artist} />

      <div className="px-6 md:px-10 py-6 flex items-center gap-4">
        <LargePlayButton />
        <ShuffleButton />
        <FollowButton />
        <DotsButton />
        <ArtistInLibraryButton artist={artist} />
      </div>

      <SectionList>
        <Section>
          <SectionHeading>Popular</SectionHeading>
          <TopArtistTracks artist={artist} />
        </Section>

        <Section>
          <SectionHeading>Albums</SectionHeading>
          <ArtistAlbumList artistId={artist.id} />
        </Section>

        <Section>
          <SectionHeading>EPs</SectionHeading>
          <ArtistEpList artistId={artist.id} />
        </Section>

        <Section>
          <SectionHeading>Singles</SectionHeading>
          <ArtistSingleList artistId={artist.id} />
        </Section>
      </SectionList>
    </GradientContent>
  );
};
