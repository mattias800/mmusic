import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useNavigate } from "react-router";

export interface AlbumCardProps {
  releaseGroup: FragmentType<typeof albumCardReleaseGroupFragment>;
}

export const albumCardReleaseGroupFragment = graphql(`
  fragment AlbumCard_ReleaseGroup on ReleaseGroup {
    id
    title
    firstReleaseYear
    credits {
      name
      artist {
        id
        name
      }
    }
    mainRelease {
      id
      coverArtUri
    }
  }
`);

export const AlbumCard: React.FC<AlbumCardProps> = (props) => {
  const releaseGroup = useFragment(
    albumCardReleaseGroupFragment,
    props.releaseGroup,
  );

  const release = releaseGroup.mainRelease;

  const navigate = useNavigate();

  if (release == null) {
    return null;
  }

  return (
    <button
      className="flex flex-col w-64 gap-2 cursor-pointer"
      onClick={() => navigate("/album/" + releaseGroup.id)}
    >
      <div className="overflow-hidden rounded-md flex flex-col gap-2 w-64">
        <img
          src={release.coverArtUri}
          alt={releaseGroup.title}
          className={
            "h-64 w-64 object-cover transition-all hover:scale-105 aspect-square"
          }
        />
      </div>
      <div className={"bold"}>{releaseGroup.title}</div>
      <div className={"text-gray-400 text-sm"}>{releaseGroup.firstReleaseYear}</div>
    </button>
  );
};
