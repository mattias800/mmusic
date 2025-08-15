import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useNavigate } from "react-router";
import { getRouteToRelease } from "@/AppRoutes.ts";
import { Tag } from "@/components/text/Tag.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";

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
    isFullyMissing
    artistName
    artist {
      id
      images {
        thumbs
      }
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
      <div className="overflow-hidden rounded-md flex flex-col gap-2 w-64">
        <ReleaseCoverArt
          srcUrl={release.coverArtUrl ?? undefined}
          artistThumbUrl={release.artist.images?.thumbs?.[0]}
          titleForPlaceholder={release.title}
          alt={release.title}
          className={
            "h-64 w-64 object-cover transition-all hover:scale-105 aspect-square"
          }
        />
      </div>
      <div className={"bold flex items-center gap-2"}>
        <span className="truncate">{release.title}</span>
        {release.isFullyMissing && <Tag variant={"error"}>Missing</Tag>}
      </div>
      <div className={"text-gray-400 text-sm"}>{release.artistName}</div>
      <div className={"text-gray-400 text-sm"}>{release.firstReleaseYear}</div>
    </button>
  );
};
