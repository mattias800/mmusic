import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { Download } from "lucide-react";
import { Button } from "@/components/ui/button.tsx";

export interface ArtistDownloadAllReleasesButtonProps {
  artist: FragmentType<typeof artistDownloadAllReleasesButtonArtistFragment>;
}

const artistDownloadAllReleasesButtonArtistFragment = graphql(`
  fragment ArtistDownloadAllReleasesButton_Artist on Artist {
    id
  }
`);

export const ArtistDownloadAllReleasesButton: React.FC<
  ArtistDownloadAllReleasesButtonProps
> = (props) => {
  const artist = useFragment(
    artistDownloadAllReleasesButtonArtistFragment,
    props.artist,
  );

  const onClickDownload = () => {
    console.log("downloading all releases for artist", artist.id);
  };

  return (
    <Button
      variant="secondary"
      onClick={onClickDownload}
      className="flex items-center gap-2"
    >
      <Download className="h-5 w-5" />
      Download all
    </Button>
  );
};
