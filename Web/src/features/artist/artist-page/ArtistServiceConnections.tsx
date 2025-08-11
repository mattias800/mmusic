import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { CircleCheckBig, CircleDashed } from "lucide-react";

export interface ArtistServiceConnectionsProps {
  artist: FragmentType<typeof artistServiceConnectionsArtistFragment>;
}

const artistServiceConnectionsArtistFragment = graphql(`
  fragment ArtistServiceConnections_Artist on Artist {
    id
    connectedExternalServices {
      isConnected
      externalService {
        id
        name
      }
    }
  }
`);

export const ArtistServiceConnections: React.FC<
  ArtistServiceConnectionsProps
> = (props) => {
  const artist = useFragment(
    artistServiceConnectionsArtistFragment,
    props.artist,
  );

  return (
    <div className="flex flex-col gap-2">
      {artist.connectedExternalServices.map((c) => (
        <span
          key={c.externalService.id}
          className={
            "px-2 py-1 rounded text-xs font-medium border flex gap-2" +
            (c.isConnected
              ? "bg-green-500/20 text-green-200 border-green-400/40"
              : "bg-white/10 text-white/70 border-white/20")
          }
          title={c.externalService.name}
        >
          {c.isConnected ? (
            <CircleCheckBig className={"w-4 h-4 mr-1"} />
          ) : (
            <CircleDashed className={"w-4 h-4 mr-1"} />
          )}
          {c.externalService.name}
        </span>
      ))}
    </div>
  );
};
