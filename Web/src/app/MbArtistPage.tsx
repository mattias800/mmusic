import { graphql } from "@/gql";
import { useQuery } from "urql";
import { useParams } from "react-router";
import { PageLoading, PageError } from "@/components/ui";
import { ArtistNotFound } from "@/app/ArtistNotFound.tsx";
import { ArtistNotInLibraryPanel } from "@/features/artist/artist-not-in-library/ArtistNotInLibraryPanel.tsx";
import { Music, AlertTriangle } from "lucide-react";

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
    return (
      <PageError
        title="Invalid Artist ID"
        message="The MusicBrainz artist ID provided is not valid"
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );
  }

  if (fetching || stale) {
    return (
      <PageLoading
        title="Loading Artist Information"
        subtitle="Fetching artist details from MusicBrainz"
        icon={Music}
        iconBgColor="bg-green-500/20"
      />
    );
  }

  if (error) {
    return (
      <PageError
        title="Failed to Load Artist"
        message="We couldn't load the artist information from MusicBrainz"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );
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
