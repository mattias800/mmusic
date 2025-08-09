import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useNavigate } from "react-router";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";

export interface MbReleaseGroupCardProps {
  releaseGroup: FragmentType<typeof mbReleaseGroupCardReleaseGroupFragment>;
}

const mbReleaseGroupCardReleaseGroupFragment = graphql(`
  fragment MbReleaseGroupCard_MbReleaseGroup on MbReleaseGroup {
    id
    title
    firstReleaseYear
    coverArtUri
  }
`);

export const MbReleaseGroupCard: React.FC<MbReleaseGroupCardProps> = (
  props,
) => {
  const releaseGroup = useFragment(
    mbReleaseGroupCardReleaseGroupFragment,
    props.releaseGroup,
  );

  const navigate = useNavigate();

  return (
    <button
      className="flex flex-col w-64 gap-2 cursor-pointer"
      onClick={() => navigate("/album/" + releaseGroup.id)}
    >
      <div className="overflow-hidden rounded-md flex flex-col gap-2 w-64">
        <ReleaseCoverArt
          srcUrl={releaseGroup.coverArtUri ?? undefined}
          titleForPlaceholder={releaseGroup.title}
          alt={releaseGroup.title}
          className={
            "h-64 w-64 object-cover transition-all hover:scale-105 aspect-square"
          }
        />
      </div>
      <div className={"bold"}>{releaseGroup.title}</div>
      <div className={"text-gray-400 text-sm"}>
        {releaseGroup.firstReleaseYear}
      </div>
    </button>
  );
};
