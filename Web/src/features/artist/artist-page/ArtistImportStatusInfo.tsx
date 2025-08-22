import * as React from "react";
import { useQuery, useSubscription } from "urql";
import { FragmentType, graphql, useFragment } from "@/gql";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { ArtistImportStatus } from "@/gql/graphql.ts";

export interface ArtistImportStatusProps {
  artist: FragmentType<typeof artistImportStatusInfoArtistFragment>;
  renderWhenNoInfo?: () => React.ReactNode;
}

const artistImportStatusInfoArtistFragment = graphql(`
  fragment ArtistImportStatusInfo_Artist on Artist {
    id
    name
  }
`);

const currentArtistImportQuery = graphql(`
  query ArtistPanel_CurrentImport {
    artistImport {
      currentArtistImport {
        id
        status
        completedReleases
        totalReleases
        errorMessage
        artistName
      }
    }
  }
`);

const currentArtistImportSubscription = graphql(`
  subscription ArtistImportStatusInfo_Sub {
    currentArtistImportUpdated {
      id
      status
      completedReleases
      totalReleases
      errorMessage
      artistName
    }
  }
`);

// Background import progress (new flow)
const backgroundProgressSubscription = graphql(`
  subscription ArtistImportStatusInfo_Background($artistId: String!) {
    artistImportBackgroundProgress(artistId: $artistId) {
      artistId
      message
      percentage
      hasError
    }
  }
`);

export const ArtistImportStatusInfo: React.FC<ArtistImportStatusProps> = ({
  renderWhenNoInfo,
  ...props
}) => {
  const artist = useFragment(
    artistImportStatusInfoArtistFragment,
    props.artist,
  );
  const [{ data }] = useQuery({
    query: currentArtistImportQuery,
  });

  // Subscribe to legacy queue progress (still supported)
  useSubscription({ query: currentArtistImportSubscription });

  // Subscribe to background progress for this artist (new flow)
  const [bgProgress] = useSubscription({
    query: backgroundProgressSubscription,
    variables: { artistId: artist.id },
    pause: !artist.id,
  });

  // If background progress is available for this artist, show it with percentage
  const bg = bgProgress.data?.artistImportBackgroundProgress;
  if (bg && bg.artistId === artist.id) {
    return (
      <div className="flex items-center gap-2 text-sm text-white/80">
        <Spinner size="sm" />
        <span>
          {bg.message} {bg.percentage != null ? `(${bg.percentage}%)` : null}
        </span>
      </div>
    );
  }

  // Fallback to legacy current-artist-import view
  const st = data?.artistImport?.currentArtistImport;

  if (!st) {
    return <>{renderWhenNoInfo?.()}</>;
  }

  const matchesArtist =
    (st.artistName ?? "").toLowerCase() === artist.name.toLowerCase();

  if (!matchesArtist) {
    return <>{renderWhenNoInfo?.()}</>;
  }

  switch (st.status) {
    case ArtistImportStatus.ImportingReleases:
      return (
        <div className="flex items-center gap-2 text-sm text-white/80">
          <Spinner size="sm" />
          <span>
            Importing releases... {st.completedReleases}/{st.totalReleases}
          </span>
        </div>
      );

    case ArtistImportStatus.ImportingArtist:
      return (
        <div className="flex items-center gap-2 text-sm text-white/80">
          <Spinner size="sm" />
          <span>Importing artist...</span>
        </div>
      );

    case ArtistImportStatus.ResolvingArtist:
      return (
        <div className="flex items-center gap-2 text-sm text-white/80">
          <Spinner size="sm" />
          <span>Resolving artist...</span>
        </div>
      );

    default:
      return <>{renderWhenNoInfo?.()}</>;
  }
};
