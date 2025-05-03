import * as React from "react";
import { cn } from "@/lib/utils.ts";
import { FragmentType, graphql, useFragment } from "@/gql";

export interface AlbumCardProps {
  release: FragmentType<typeof albumCardReleaseFragment>;
}

export const albumCardReleaseFragment = graphql(`
  fragment AlbumCard_Release on Release {
    id
    title
    coverArtUri
    artists {
      id
      name
    }
  }
`);

export const AlbumCard: React.FC<AlbumCardProps> = (props) => {
  const release = useFragment(albumCardReleaseFragment, props.release);

  return (
    <div className="overflow-hidden rounded-md">
      <img
        src={release.coverArtUri}
        alt={release.title}
        className={cn(
          "h-auto w-auto object-cover transition-all hover:scale-105",
          "portrait" === "portrait" ? "aspect-[3/4]" : "aspect-square",
        )}
      />
      <span>{release.artists[0].name}</span>
    </div>
  );
};
