import * as React from "react";
import { Link } from "react-router";
import { FragmentType, graphql, useFragment } from "@/gql";

export interface TrackCreditLinksProps {
  track: FragmentType<typeof trackCreditLinksTrackFragment>;
}

const trackCreditLinksTrackFragment = graphql(`
  fragment TrackCreditLinks_Track on Track {
    id
    credits {
      artistName
      artist {
        id
        name
      }
      mbArtist {
        id
        name
      }
    }
  }
`);

export const TrackCreditLinks: React.FC<TrackCreditLinksProps> = (props) => {
  const track = useFragment(trackCreditLinksTrackFragment, props.track);

  return (
    <>
      {track.credits.map(({ artist, mbArtist, artistName }, index) => (
        <React.Fragment key={index}>
          {index > 0 && ", "}
          {artist ? (
            <Link to={`/artist/${artist.id}`} className="hover:underline">
              {artistName}
            </Link>
          ) : mbArtist ? (
            <Link to={`/mb-artist/${mbArtist.id}`} className="hover:underline">
              {artistName}
            </Link>
          ) : (
            <span>{artistName}</span>
          )}
        </React.Fragment>
      ))}
    </>
  );
};
