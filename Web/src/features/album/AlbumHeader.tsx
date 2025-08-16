import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { sumBy } from "lodash-es";
import { formatAlbumLength } from "@/common/AlbumLengthFormatter.ts";
import { ReleaseType } from "@/gql/graphql.ts";
import { getRouteToArtist } from "@/AppRoutes.ts";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { Tag } from "@/components/text/Tag.tsx";
import { Music, Calendar, Clock, User } from "lucide-react";

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
    artistName
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
    <div className="space-y-8">
      {/* Main Header Section */}
      <div className="flex flex-col lg:flex-row gap-8 items-center lg:items-start">
        {/* Cover Art */}
        <div className="relative group">
          <ReleaseCoverArt
            srcUrl={release.coverArtUrl ?? undefined}
            artistThumbUrl={release.artist.images?.thumbs?.[0]}
            titleForPlaceholder={release.title}
            alt={release.title + " album cover"}
            className="w-72 h-72 lg:w-80 lg:h-80 rounded-2xl shadow-2xl object-cover border-2 border-white/20 transition-all duration-300 group-hover:border-white/40 group-hover:shadow-3xl"
          />
          <div className="absolute inset-0 rounded-2xl bg-gradient-to-t from-black/20 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
        </div>

        {/* Release Info */}
        <div className="flex flex-col gap-6 flex-1 min-w-0">
          {/* Release Type Tag */}
          {getReleaseType(release.type) && (
            <div className="flex items-center gap-2">
              <div className="p-2 bg-gradient-to-br from-blue-500/20 to-purple-500/20 rounded-xl border border-blue-500/30">
                <Music className="w-4 h-4 text-blue-400" />
              </div>
              <Tag variant="info">{getReleaseType(release.type)}</Tag>
            </div>
          )}

          {/* Title */}
          <h1 className="text-4xl lg:text-6xl xl:text-7xl font-bold text-white leading-tight tracking-tight">
            {release.title}
          </h1>

          {/* Artist Link */}
          <div className="flex items-center gap-3">
            <div className="p-2 bg-gradient-to-br from-green-500/20 to-emerald-500/20 rounded-xl border border-green-500/30">
              <User className="w-4 h-4 text-green-400" />
            </div>
            <Link
              className="text-xl font-semibold text-white hover:text-green-300 transition-colors duration-200 hover:underline"
              to={getRouteToArtist(release.artist.id)}
            >
              {release.artistName !== release.artist.name
                ? `${release.artistName} (${release.artist.name})`
                : release.artistName}
            </Link>
          </div>

          {/* Release Details */}
          <div className="flex flex-wrap items-center gap-6 text-gray-300">
            {/* Release Year */}
            <div className="flex items-center gap-2">
              <div className="p-1.5 bg-gradient-to-br from-yellow-500/20 to-orange-500/20 rounded-lg border border-yellow-500/30">
                <Calendar className="w-4 h-4 text-yellow-400" />
              </div>
              <span className="font-medium">{release.firstReleaseYear}</span>
            </div>

            {/* Track Count */}
            <div className="flex items-center gap-2">
              <div className="p-1.5 bg-gradient-to-br from-purple-500/20 to-pink-500/20 rounded-lg border border-purple-500/30">
                <Music className="w-4 h-4 text-purple-400" />
              </div>
              <span className="font-medium">{numSongsText}</span>
            </div>

            {/* Duration */}
            <div className="flex items-center gap-2">
              <div className="p-1.5 bg-gradient-to-br from-indigo-500/20 to-blue-500/20 rounded-lg border border-indigo-500/30">
                <Clock className="w-4 h-4 text-indigo-400" />
              </div>
              <span className="font-medium">{albumTime}</span>
            </div>
          </div>
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
