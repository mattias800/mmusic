import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";
import { PageLayout } from "@/components/ui";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs.tsx";

export interface AlbumListProps {
  releaseGroups: Array<FragmentType<typeof albumListAlbumFragment>>;
}

const albumListAlbumFragment = graphql(`
  fragment AlbumList_Release on Release {
    id
    isFullyMissing
    ...AlbumCard_Release
  }
`);

export const AlbumList: React.FC<AlbumListProps> = (props) => {
  const releaseGroup = useFragment(albumListAlbumFragment, props.releaseGroups);
  const [showMode, setShowMode] = React.useState<"all" | "withAudio">("all");

  const filteredReleases = React.useMemo(
    () =>
      releaseGroup.filter((album) =>
        showMode === "all" ? true : !album.isFullyMissing,
      ),
    [releaseGroup, showMode],
  );

  return (
    <PageLayout addSearchPadding>
      <SectionList>
        <Section>
          <div className="flex items-center justify-between mb-2">
            <SectionHeading>Albums</SectionHeading>
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <span className="hidden sm:inline">Show</span>
              <Tabs
                value={showMode}
                onValueChange={(v) =>
                  setShowMode(v === "withAudio" ? "withAudio" : "all")
                }
              >
                <TabsList>
                  <TabsTrigger value="all">All</TabsTrigger>
                  <TabsTrigger value="withAudio">With audio</TabsTrigger>
                </TabsList>
              </Tabs>
            </div>
          </div>
          <CardFlexList>
            {filteredReleases.map((album) => (
              <AlbumCard release={album} key={album.id} />
            ))}
          </CardFlexList>
        </Section>
      </SectionList>
    </PageLayout>
  );
};
