import * as React from "react";
import { Check } from "lucide-react";
import { useMutation } from "urql";
import { graphql } from "@/gql";
import { GradientButton } from "@/components/ui";
import { useNavigate } from "react-router";
import { getRouteToArtist } from "@/AppRoutes.ts";

export interface ArtistInLibraryButtonProps {
  artistId: string;
  isInLibrary: boolean;
}

const importArtistMutation = graphql(`
  mutation ImportArtist($musicBrainzArtistId: String!) {
    importArtist(input: { musicBrainzArtistId: $musicBrainzArtistId }) {
      __typename
      ... on ImportArtistSuccess {
        artist {
          id
          name
        }
      }
      ... on ImportArtistError {
        message
      }
    }
  }
`);

export const ArtistInLibraryButton: React.FC<ArtistInLibraryButtonProps> = ({
  artistId,
  isInLibrary,
}) => {
  const navigate = useNavigate();
  const [{ fetching }, importArtist] = useMutation(importArtistMutation);

  const onClick = async () => {
    if (fetching) return;
    const res = await importArtist({ musicBrainzArtistId: artistId });
    const payload = res.data?.importArtist;
    if (payload && payload.__typename === "ImportArtistSuccess") {
      const newId = payload.artist.id;
      // Navigate to the newly created artist page so UI can render immediately
      navigate(getRouteToArtist(newId));
    }
    // Errors are surfaced via existing UI patterns; no-op here
  };

  if (isInLibrary) {
    return (
      <GradientButton onClick={onClick} loading={fetching} iconLeft={Check}>
        In Library
      </GradientButton>
    );
  }

  return (
    <GradientButton onClick={onClick} loading={fetching}>
      Add to Library
    </GradientButton>
  );
};
