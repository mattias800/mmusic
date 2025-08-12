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
      artistPageUrl
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
      {artist.connectedExternalServices.map((c) => {
        const classNameBase =
          "px-2 py-1 rounded text-xs font-medium border flex gap-2 items-center" +
          (c.isConnected
            ? " bg-green-500/20 text-green-200 border-green-400/40"
            : " bg-white/10 text-white/70 border-white/20");

        if (c.isConnected && c.artistPageUrl) {
          return (
            <a
              key={c.externalService.id}
              href={c.artistPageUrl}
              target="_blank"
              rel="noopener noreferrer"
              className={classNameBase + " hover:underline"}
              title={c.externalService.name}
            >
              <CircleCheckBig className={"w-4 h-4 mr-1"} />
              {c.externalService.name}
            </a>
          );
        }

        return (
          <span key={c.externalService.id} className={classNameBase} title={c.externalService.name}>
            <CircleDashed className={"w-4 h-4 mr-1"} />
            {c.externalService.name}
          </span>
        );
      })}
    </div>
  );
};
