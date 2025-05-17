import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { byStringField } from "@/common/sorting/Comparators.ts";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";

export interface ArtistSingleListProps {
  artist: FragmentType<typeof artistSingleListArtistFragment>;
}

export const artistSingleListArtistFragment = graphql(`
  fragment ArtistSingleList_Artist on Artist {
    id
    singles {
      id
      firstReleaseDate
      ...AlbumCard_ReleaseGroup
    }
  }
`);

export const ArtistSingleList: React.FC<ArtistSingleListProps> = (props) => {
  const artist = useFragment(artistSingleListArtistFragment, props.artist);

  return (
    <div className={"flex flex-wrap gap-8"}>
      {artist.singles
        .toSorted(byStringField((a) => a.firstReleaseDate ?? ""))
        .toReversed()
        .map((release) => (
          <AlbumCard releaseGroup={release} key={release.id} />
        ))}
    </div>
  );
};
