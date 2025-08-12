import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { sumBy } from "lodash-es";
import { formatAlbumLength } from "@/common/AlbumLengthFormatter.ts";
import { ReleaseType } from "@/gql/graphql.ts";
import { getRouteToArtist } from "@/AppRoutes.ts";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { Tag } from "@/components/text/Tag.tsx";

export interface AlbumHeaderProps {
  release: FragmentType<typeof albumHeaderReleaseFragment>;
}

const albumHeaderReleaseFragment = graphql(`
  fragment AlbumHeader_Release on Release {
    id
    title
    type
    coverArtUrl
    firstReleaseYear
    artist {
      id
      name
      images {
        thumbs
      }
    }
    tracks {
      id
      trackLength
    }
  }
`);

export const AlbumHeader: React.FC<AlbumHeaderProps> = (props) => {
  const release = useFragment(albumHeaderReleaseFragment, props.release);

  const numTracks = release?.tracks.length ?? 0;

  const numSongsText = numTracks === 1 ? "1 song" : `${numTracks} songs`;
  const albumTime = formatAlbumLength(
    sumBy(release?.tracks ?? [], (track) => track.trackLength ?? 0),
  );

  if (!release) {
    return null;
  }

  return (
    <div className="flex flex-col sm:flex-row gap-6 items-center sm:items-end mb-8">
      <ReleaseCoverArt
        srcUrl={release.coverArtUrl ?? undefined}
        artistThumbUrl={release.artist.images?.thumbs?.[0]}
        titleForPlaceholder={release.title}
        alt={release.title + " album cover"}
        className="w-64 h-64 rounded shadow-lg object-cover"
      />
      <div className={"flex flex-col gap-3"}>
        {getReleaseType(release.type) && (
          <div>
            <Tag>{getReleaseType(release.type)}</Tag>
          </div>
        )}
        <h1 className="text-4xl sm:text-7xl font-bold">{release.title}</h1>
        <div className={"flex gap-2"}>
          <Link
            className={"text-sm font-bold"}
            to={getRouteToArtist(release.artist.id)}
          >
            {release.artist.name}
          </Link>
          <p className="text-sm text-gray-300">
            • {release.firstReleaseYear} • {numSongsText}, {albumTime}
          </p>
        </div>
      </div>
    </div>
  );
};

const getReleaseType = (
  releaseType: ReleaseType | null | undefined,
): string | undefined => {
  switch (releaseType) {
    case ReleaseType.Album:
      return "Album";
    case ReleaseType.Ep:
      return "EP";
    case ReleaseType.Single:
      return "Single";
    default:
      return undefined;
  }
};
