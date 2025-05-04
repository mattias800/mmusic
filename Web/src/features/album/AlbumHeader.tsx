import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { Link } from "react-router";

export interface AlbumHeaderProps {
  release: FragmentType<typeof albumHeaderReleaseFragment>;
}

export const albumHeaderReleaseFragment = graphql(`
  fragment AlbumHeader_Release on Release {
    id
    title
    artists {
      id
      name
    }
  }
`);

export const AlbumHeader: React.FC<AlbumHeaderProps> = (props) => {
  const release = useFragment(albumHeaderReleaseFragment, props.release);

  return (
    <div className="flex flex-col sm:flex-row gap-6 items-center sm:items-end mb-8">
      <img
        src="https://cdn-images.dzcdn.net/images/cover/2a6ba6be3606384210b67130a70a7c4e/1900x1900-000000-80-0-0.jpg"
        alt={release.title + " album cover"}
        className="w-48 h-48 rounded shadow-lg"
      />
      <div className={"flex flex-col gap-3"}>
        <p className="text-sm">Album</p>
        <h1 className="text-4xl sm:text-7xl font-bold">{release.title}</h1>
        <div className={"flex gap-2"}>
          <Link
            className={"text-sm font-bold"}
            to={"/artist/" + release.artists[0].id}
          >
            {release.artists[0].name}
          </Link>
          <p className="text-sm text-gray-300">
            • 1995 • 9 songs, 43 min 55 sec
          </p>
        </div>
      </div>
    </div>
  );
};
