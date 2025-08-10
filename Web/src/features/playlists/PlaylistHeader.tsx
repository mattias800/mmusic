import * as React from "react";
import { useMemo } from "react";
import { FragmentType, graphql, useFragment } from "@/gql";

export interface PlaylistHeaderProps {
  playlist: FragmentType<typeof playlistHeaderPlaylistFragment>;
}

const playlistHeaderPlaylistFragment = graphql(`
  fragment PlaylistHeader_Playlist on Playlist {
    id
    name
    items {
      id
      coverImageUrl
    }
  }
`);

export const PlaylistHeader: React.FC<PlaylistHeaderProps> = (props) => {
  const playlist = useFragment(playlistHeaderPlaylistFragment, props.playlist);

  const uniqueCoverUrls = useMemo(() => {
    const seen = new Set<string>();
    const urls: string[] = [];
    for (const t of playlist.items) {
      const url = t.coverImageUrl ?? "";
      if (url && !seen.has(url)) {
        seen.add(url);
        urls.push(url);
      }
    }
    return urls;
  }, [playlist.items]);

  const collageUrls = useMemo(
    () => uniqueCoverUrls.slice(0, 4),
    [uniqueCoverUrls],
  );

  return (
    <div className="flex flex-col sm:flex-row gap-6 items-center sm:items-end mb-8">
      <div>
        {collageUrls.length >= 4 ? (
          <div className="grid grid-cols-2 grid-rows-2 w-64 h-64 overflow-hidden rounded-md ring-1 ring-white/10">
            {collageUrls.map((url, i) => (
              <img
                key={i}
                src={url}
                alt={`Cover ${i + 1}`}
                className="w-full h-full object-cover"
                draggable={false}
              />
            ))}
          </div>
        ) : (
          <div className="w-64 h-64">
            {uniqueCoverUrls[0] ? (
              <img
                src={uniqueCoverUrls[0]}
                alt="Playlist cover"
                className="w-64 h-64 object-cover rounded-md ring-1 ring-white/10"
                draggable={false}
              />
            ) : (
              <div className="w-64 h-64 rounded-md bg-neutral-800 ring-1 ring-white/10" />
            )}
          </div>
        )}
      </div>

      <div className={"flex flex-col gap-3"}>
        <h1 className="text-4xl font-semibold">
          {playlist.name ?? "Playlist"}
        </h1>
        <div className="text-sm text-gray-300">
          {playlist.items.length}{" "}
          {playlist.items.length === 1 ? "track" : "tracks"}
        </div>
      </div>
    </div>
  );
};
