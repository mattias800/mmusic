import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";
import { PageLayout } from "@/components/ui";

export interface AlbumListProps {
  releaseGroups: Array<FragmentType<typeof albumListAlbumFragment>>;
}

const albumListAlbumFragment = graphql(`
  fragment AlbumList_Release on Release {
    id
    ...AlbumCard_Release
  }
`);

export const AlbumList: React.FC<AlbumListProps> = (props) => {
  const releaseGroup = useFragment(albumListAlbumFragment, props.releaseGroups);

  return (
    <PageLayout addSearchPadding>
      <SectionList>
        <Section>
          <SectionHeading>Albums</SectionHeading>
          <CardFlexList>
            {releaseGroup.map((album) => (
              <AlbumCard release={album} key={album.id} />
            ))}
          </CardFlexList>
        </Section>
      </SectionList>
    </PageLayout>
  );
};
