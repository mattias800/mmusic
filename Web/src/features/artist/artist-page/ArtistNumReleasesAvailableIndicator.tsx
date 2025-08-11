import * as React from "react";
import { CheckCircle, CircleAlert } from "lucide-react";
import { FragmentType, graphql, useFragment } from "@/gql";

export interface ArtistNumReleasesAvailableIndicatorProps {
  artist: FragmentType<
    typeof artistNumReleasesAvailableIndicatorArtistFragment
  >;
  renderDownloadAllReleasesButton?: () => React.ReactNode;
}

const artistNumReleasesAvailableIndicatorArtistFragment = graphql(`
  fragment ArtistNumReleasesAvailableIndicator_Artist on Artist {
    id
    albums {
      id
      isFullyMissing
    }
    eps {
      id
      isFullyMissing
    }
    singles {
      id
      isFullyMissing
    }
  }
`);

export const ArtistNumReleasesAvailableIndicator: React.FC<
  ArtistNumReleasesAvailableIndicatorProps
> = ({ renderDownloadAllReleasesButton, ...props }) => {
  const artist = useFragment(
    artistNumReleasesAvailableIndicatorArtistFragment,
    props.artist,
  );

  const totalNumReleases =
    artist.albums.length + artist.eps.length + artist.singles.length;

  const availableNumReleases =
    artist.albums.filter((r) => !r.isFullyMissing).length +
    artist.eps.filter((r) => !r.isFullyMissing).length +
    artist.singles.filter((r) => !r.isFullyMissing).length;

  return (
    <>
      {totalNumReleases != null && totalNumReleases > 0 && (
        <div className="text-white text-sm flex items-center gap-2">
          {availableNumReleases === totalNumReleases ? (
            <>
              <CheckCircle className="w-4 h-4 text-green-400" />
              <span>Available</span>
            </>
          ) : (
            <div className={"flex items-center gap-4"}>
              <span className={"flex items-center gap-2"}>
                <CircleAlert className={"w-4 h-4 text-yellow-400"} />
                {availableNumReleases} of {totalNumReleases} releases available
              </span>
              {renderDownloadAllReleasesButton?.()}
            </div>
          )}
        </div>
      )}
    </>
  );
};
