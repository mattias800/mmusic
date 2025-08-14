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

  useSubscription({ query: currentArtistImportSubscription });

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
