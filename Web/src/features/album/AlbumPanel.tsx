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
  releaseGroup: FragmentType<typeof albumPanelReleaseGroupFragment>;
}

const albumPanelReleaseGroupFragment = graphql(`
  fragment AlbumPanel_ReleaseGroup on ReleaseGroup {
    id
    ...AlbumHeader_ReleaseGroup
    ...AlbumTrackList_ReleaseGroup
    firstReleaseYear
    mainRelease {
      labels {
        id
        name
      }
    }
  }
`);

export const AlbumPanel: React.FC<AlbumPanelProps> = (props) => {
  const releaseGroup = useFragment(
    albumPanelReleaseGroupFragment,
    props.releaseGroup,
  );

  return (
    <GradientContent>
      <MainPadding>
        <div>
          <div>
            <AlbumHeader releaseGroup={releaseGroup} />

            <div className="flex items-center gap-6 mb-6">
              <LargePlayButton />
              <ShuffleButton />
              <LargeLikeButton />
              <DotsButton />
            </div>
          </div>

          <div>
            <AlbumTrackList releaseGroup={releaseGroup} />

            {releaseGroup.mainRelease &&
              releaseGroup.mainRelease.labels.length > 0 && (
                <div className="text-white/40 text-xs mt-12">
                  {releaseGroup.mainRelease.labels.map((label) => (
                    <p>
                      © {releaseGroup.firstReleaseYear} {label.name}
                    </p>
                  ))}
                </div>
              )}
          </div>
        </div>
      </MainPadding>
    </GradientContent>
  );
};
