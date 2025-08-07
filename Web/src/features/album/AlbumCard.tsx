import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useNavigate } from "react-router";
import { getRouteToRelease } from "@/AppRoutes.ts";

export interface AlbumCardProps {
  release: FragmentType<typeof albumCardReleaseGroupFragment>;
}

const albumCardReleaseGroupFragment = graphql(`
  fragment AlbumCard_Release on Release {
    id
    title
    firstReleaseYear
    coverArtUrl
    folderName
    artist {
      id
    }
  }
`);

export const AlbumCard: React.FC<AlbumCardProps> = (props) => {
  const release = useFragment(albumCardReleaseGroupFragment, props.release);

  const navigate = useNavigate();

  return (
    <button
      className="flex flex-col w-64 gap-2 cursor-pointer"
      onClick={() =>
        navigate(getRouteToRelease(release.artist.id, release.folderName))
      }
    >
      {release.coverArtUrl && (
        <div className="overflow-hidden rounded-md flex flex-col gap-2 w-64">
          <img
            src={release.coverArtUrl}
            alt={release.title}
            className={
              "h-64 w-64 object-cover transition-all hover:scale-105 aspect-square"
            }
          />
        </div>
      )}
      <div className={"bold"}>{release.title}</div>
      <div className={"text-gray-400 text-sm"}>{release.firstReleaseYear}</div>
    </button>
  );
};
