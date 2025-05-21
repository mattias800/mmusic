import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";

export interface ArtistCardProps {
  artist: FragmentType<typeof artistCardArtistFragment>;
}

export const artistCardArtistFragment = graphql(`
  fragment ArtistCard_Artist on Artist {
    id
    name
    images {
      artistThumb
    }
  }
`);

export const ArtistCard: React.FC<ArtistCardProps> = (props) => {
  const artist = useFragment(artistCardArtistFragment, props.artist);
  return <div>{artist.name}</div>;
};
