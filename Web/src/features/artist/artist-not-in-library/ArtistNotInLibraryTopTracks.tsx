import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { formatLargeNumber } from "@/common/TrackLengthFormatter.ts";

export interface ArtistNotInLibraryTopTracksProps {
  artist: FragmentType<typeof artistNotInLibraryTopTracksArtistFragment>;
}

const artistNotInLibraryTopTracksArtistFragment = graphql(`
  fragment ArtistNotInLibraryTopTracks_Artist on Artist {
    id
    topTracks {
      id
      name
      statistics {
        listeners
      }
    }
  }
`);

export const ArtistNotInLibraryTopTracks: React.FC<
  ArtistNotInLibraryTopTracksProps
> = (props) => {
  const artist = useFragment(
    artistNotInLibraryTopTracksArtistFragment,
    props.artist,
  );

  const topTracks = artist.topTracks.slice(0, 3);

  return (
    <div className={"flex flex-col grow"}>
      {topTracks.map((track, idx) => (
        <div
          className={"flex items-center gap-4 p-4  rounded-md justify-between"}
        >
          <div className={"flex gap-4 items-center"}>
            <div className={"w-8"}>{idx}</div>
            <div>{track.name}</div>
          </div>
          <span className="text-sm text-neutral-400 text-right">
            {formatLargeNumber(track.statistics.listeners)} listeners
          </span>
        </div>
      ))}
    </div>
  );
};
