import * as React from "react";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import { LargeLikeButton } from "@/components/buttons/LargeLikeButton.tsx";
import { AlbumHeader } from "@/features/album/AlbumHeader.tsx";
import { AlbumTrackList } from "@/features/album/AlbumTrackList.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { GradientContent } from "@/components/page-body/GradientContent";
import { MainPadding } from "@/components/layout/MainPadding.tsx";

export interface AlbumPanelProps {
  release: FragmentType<typeof albumPanelReleaseGroupFragment>;
}

const albumPanelReleaseGroupFragment = graphql(`
  fragment AlbumPanel_Release on Release {
    id
    ...AlbumHeader_Release
    ...AlbumTrackList_Release
    firstReleaseYear
  }
`);

export const AlbumPanel: React.FC<AlbumPanelProps> = (props) => {
  const release = useFragment(albumPanelReleaseGroupFragment, props.release);

  return (
    <GradientContent>
      <MainPadding>
        <div>
          <div>
            <AlbumHeader release={release} />

            <div className="flex items-center gap-6 mb-6">
              <LargePlayButton />
              <ShuffleButton />
              <LargeLikeButton />
              <DotsButton />
            </div>
          </div>

          <div>
            <AlbumTrackList releaseGroup={release} />

            <div className="text-white/40 text-xs mt-12">
              <p>© {release.firstReleaseYear} Some label AB</p>
            </div>
          </div>
        </div>
      </MainPadding>
    </GradientContent>
  );
};
