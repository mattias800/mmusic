import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { formatLargeNumber } from "@/common/TrackLengthFormatter.ts";
import { useNavigate } from "react-router";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { PhotoCardBottomText } from "@/components/cards/PhotoCardBottomText.tsx";
import { Check, CheckCheck } from "lucide-react";

export interface ArtistCardProps {
  artist: FragmentType<typeof artistCardArtistFragment>;
}

const artistCardArtistFragment = graphql(`
  fragment ArtistCard_Artist on Artist {
    id
    name
    listeners
    images {
      thumbs
    }
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

export const ArtistCard: React.FC<ArtistCardProps> = (props) => {
  const artist = useFragment(artistCardArtistFragment, props.artist);
  const imageUrl = artist.images?.thumbs?.[0];

  const navigate = useNavigate();
  const totalAlbums = artist.albums?.length ?? 0;
  const availableAlbums =
    artist.albums?.filter((r) => !r.isFullyMissing).length ?? 0;
  const allAlbumsAvailable = totalAlbums > 0 && availableAlbums === totalAlbums;

  const totalReleases =
    (artist.albums?.length ?? 0) +
    (artist.eps?.length ?? 0) +
    (artist.singles?.length ?? 0);
  const availableReleases =
    (artist.albums?.filter((r) => !r.isFullyMissing).length ?? 0) +
    (artist.eps?.filter((r) => !r.isFullyMissing).length ?? 0) +
    (artist.singles?.filter((r) => !r.isFullyMissing).length ?? 0);
  const allReleasesAvailable =
    totalReleases > 0 && availableReleases === totalReleases;

  return (
    <PhotoCard
      imageUrl={imageUrl ?? ""}
      imageAlt={artist.name + " cover"}
      onClick={() => navigate(`/artist/${artist.id}`)}
    >
      <PhotoCardCenterHeading>{artist.name}</PhotoCardCenterHeading>
      {(allReleasesAvailable || allAlbumsAvailable) && (
        <div className="absolute bottom-3 left-4 z-20">
          {allReleasesAvailable ? (
            <CheckCheck className="w-4 h-4 text-green-400" />
          ) : (
            <Check className="w-4 h-4 text-green-400" />
          )}
        </div>
      )}
      <PhotoCardBottomText>
        {`${formatLargeNumber(artist.listeners)} listeners`}
      </PhotoCardBottomText>
    </PhotoCard>
  );
};
