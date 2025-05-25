import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { ArtistNotAvailableInMusicBrainz } from "@/features/artist/artist-not-in-library/ArtistNotAvailableInMusicBrainz.tsx";
import { ArtistNotInLibraryPanel } from "@/features/artist/artist-not-in-library/ArtistNotInLibraryPanel.tsx";

export interface ArtistNotInLibraryFetcherProps {
  artistId: string;
}

const artistNotInLibraryFetcherQuery = graphql(`
  query ArtistNotInLibraryFetcher($artistId: ID!) {
    musicBrainz {
      artist {
        byId(id: $artistId) {
          id
          ...ArtistNotInLibraryPanel_MbArtist
        }
      }
    }
  }
`);

export const ArtistNotInLibraryFetcher: React.FC<
  ArtistNotInLibraryFetcherProps
> = ({ artistId }) => {
  const [{ error, data, fetching, stale }] = useQuery({
    query: artistNotInLibraryFetcherQuery,
    variables: { artistId: artistId! },
  });

  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.musicBrainz.artist.byId)
    return <ArtistNotAvailableInMusicBrainz />;

  return <ArtistNotInLibraryPanel artist={data.musicBrainz.artist.byId} />;
};
