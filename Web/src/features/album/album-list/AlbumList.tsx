import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";
import { MainPadding } from "@/components/layout/MainPadding.tsx";

export interface AlbumListProps {
  releaseGroups: Array<FragmentType<typeof albumListAlbumFragment>>;
}

export const albumListAlbumFragment = graphql(`
  fragment AlbumList_ReleaseGroup on ReleaseGroup {
    id
    ...AlbumCard_ReleaseGroup
  }
`);

export const AlbumList: React.FC<AlbumListProps> = (props) => {
  const releaseGroup = useFragment(albumListAlbumFragment, props.releaseGroups);

  return (
    <MainPadding>
      <SectionList>
        <Section>
          <SectionHeading>Albums</SectionHeading>
          <CardFlexList>
            {releaseGroup.map((album) => (
              <AlbumCard releaseGroup={album} key={album.id} />
            ))}
          </CardFlexList>
        </Section>
      </SectionList>
    </MainPadding>
  );
};
