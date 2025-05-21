import * as React from "react";
import { LargePlayButton } from "@/common/components/buttons/LargePlayButton.tsx";
import { ShuffleButton } from "@/common/components/buttons/ShuffleButton.tsx";
import { DotsButton } from "@/common/components/buttons/DotsButton.tsx";
import { LargeLikeButton } from "@/common/components/buttons/LargeLikeButton.tsx";
import { AlbumHeader } from "@/features/album/AlbumHeader.tsx";
import { PageCard } from "@/common/components/PageCard.tsx";
import { AlbumTrackList } from "@/features/album/AlbumTrackList.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";

export interface AlbumPanelProps {
  releaseGroup: FragmentType<typeof albumPanelReleaseGroupFragment>;
}

export const albumPanelReleaseGroupFragment = graphql(`
  fragment AlbumPanel_ReleaseGroup on ReleaseGroup {
    id
    ...AlbumHeader_ReleaseGroup
    ...AlbumTrackList_ReleaseGroup
  }
`);

export const AlbumPanel: React.FC<AlbumPanelProps> = (props) => {
  const releaseGroup = useFragment(albumPanelReleaseGroupFragment, props.releaseGroup);

  return (
    <PageCard
      header={
        <>
          <AlbumHeader releaseGroup={releaseGroup}/>

          <div className="flex items-center gap-6 mb-6">
            <LargePlayButton />
            <ShuffleButton />
            <LargeLikeButton />
            <DotsButton />
          </div>
        </>
      }
    >
      <AlbumTrackList releaseGroup={releaseGroup}/>

      <div className="text-white/40 text-xs mt-12">
        <p>© 1995 King Size Records</p>
        <p>℗ 1995 King Size Records</p>
      </div>
    </PageCard>
  );
};
