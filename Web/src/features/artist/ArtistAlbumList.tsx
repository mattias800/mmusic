import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";
import { byStringField } from "@/common/sorting/Comparators.ts";

export interface ArtistAlbumListProps {
  artist: FragmentType<typeof artistAlbumListArtistFragment>;
}

export const artistAlbumListArtistFragment = graphql(`
  fragment ArtistAlbumList_Artist on Artist {
    id
    mainAlbums {
      id
      date
      year
      country
      releaseGroup {
        id
        primaryType
        secondaryTypes
      }
      ...AlbumCard_Release
    }
  }
`);

export const ArtistAlbumList: React.FC<ArtistAlbumListProps> = (props) => {
  const artist = useFragment(artistAlbumListArtistFragment, props.artist);

  return (
    <div className={"flex flex-wrap gap-8"}>
      {artist.mainAlbums
        .toSorted(byStringField((a) => a.year ?? ""))
        .toReversed()
        .map((release) => (
          <>
            <AlbumCard release={release} />
          </>
        ))}
    </div>
  );
};
