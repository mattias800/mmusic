import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { sumBy } from "lodash-es";
import { formatAlbumLength } from "@/common/AlbumLengthFormatter.ts";

export interface AlbumHeaderProps {
  releaseGroup: FragmentType<typeof albumHeaderReleaseGroupFragment>;
}

export const albumHeaderReleaseGroupFragment = graphql(`
  fragment AlbumHeader_ReleaseGroup on ReleaseGroup {
    id
    title
    mainRelease {
      id
      title
      coverArtUri
      year

      credits {
        artist {
          id
          name
        }
      }
      recordings {
        id
        length
      }
    }
  }
`);

export const AlbumHeader: React.FC<AlbumHeaderProps> = (props) => {
  const releaseGroup = useFragment(
    albumHeaderReleaseGroupFragment,
    props.releaseGroup,
  );

  const release = releaseGroup.mainRelease;
  const numTracks = release?.recordings.length ?? 0;

  const numSongsText = numTracks === 1 ? "1 song" : `${numTracks} songs`;
  const albumTime = formatAlbumLength(
    sumBy(release?.recordings ?? [], (recording) => recording.length ?? 0),
  );

  if (!release) {
    return null;
  }

  return (
    <div className="flex flex-col sm:flex-row gap-6 items-center sm:items-end mb-8">
      <img
        src={release.coverArtUri}
        alt={release.title + " album cover"}
        className="w-48 h-48 rounded shadow-lg"
      />
      <div className={"flex flex-col gap-3"}>
        <p className="text-sm">Album</p>
        <h1 className="text-4xl sm:text-7xl font-bold">{release.title}</h1>
        <div className={"flex gap-2"}>
          <Link
            className={"text-sm font-bold"}
            to={"/artist/" + release.credits[0].artist.id}
          >
            {release.credits[0].artist.name}
          </Link>
          <p className="text-sm text-gray-300">
            • {release.year} • {numSongsText}, {albumTime}
          </p>
        </div>
      </div>
    </div>
  );
};
