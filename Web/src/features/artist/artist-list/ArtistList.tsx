import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { ArtistCard } from "@/features/artist/artist-card/ArtistCard.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { PageLayout } from "@/components/ui";

export interface ArtistListProps {
  artists: Array<FragmentType<typeof artistListArtistFragment>>;
}

const artistListArtistFragment = graphql(`
  fragment ArtistList_Artist on Artist {
    id
    ...ArtistCard_Artist
  }
`);

export const ArtistList: React.FC<ArtistListProps> = (props) => {
  const artists = useFragment(artistListArtistFragment, props.artists);

  return (
    <PageLayout addSearchPadding>
      <SectionList>
        <Section>
          <SectionHeading>Artists</SectionHeading>
          <CardFlexList>
            {artists.map((artist) => (
              <ArtistCard artist={artist} key={artist.id} />
            ))}
          </CardFlexList>
        </Section>
      </SectionList>
    </PageLayout>
  );
};
