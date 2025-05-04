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
    albums {
      id
      date
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
      {artist.albums
        .toSorted(byStringField((a) => a.date))
        .toReversed()
        .map((release) => (
          <>
            <AlbumCard release={release} />
            {release.date}
            {release.country}
            {JSON.stringify(release.releaseGroup)}
          </>
        ))}
    </div>
  );
};
