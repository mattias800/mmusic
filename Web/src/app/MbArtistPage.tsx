import { graphql } from "@/gql";
import { useQuery } from "urql";
import { useParams } from "react-router";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { ArtistNotFound } from "@/app/ArtistNotFound.tsx";
import { ArtistNotInLibraryPanel } from "@/features/artist/artist-not-in-library/ArtistNotInLibraryPanel.tsx";

const mbArtistQuery = graphql(`
  query MbArtistQuery($mbArtistId: ID!) {
    musicBrainz {
      artist {
        byId(id: $mbArtistId) {
          id
          name
          ...ArtistNotInLibraryPanel_MbArtist
        }
      }
    }
  }
`);

export const MbArtistPage = () => {
  const { mbArtistId } = useParams<{ mbArtistId: string }>();
  const [{ error, data, fetching, stale }] = useQuery({
    query: mbArtistQuery,
    variables: { mbArtistId: mbArtistId! },
    pause: !mbArtistId,
  });

  if (!mbArtistId) {
    return "Invalid artist ID";
  }

  if (fetching || stale) {
    return <ScreenSpinner />;
  }
  if (error) {
    return <div>Error: {error.message}</div>;
  }
  if (!data?.musicBrainz.artist.byId) {
    return <ArtistNotFound />;
  }

  return (
    <>
      <title>{data.musicBrainz.artist.byId.name}</title>
      <ArtistNotInLibraryPanel artist={data.musicBrainz.artist.byId} />
    </>
  );
};
