import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { Tag } from "@/components/text/Tag.tsx";
import { MediaAvailabilityStatus } from "@/gql/graphql.ts";

export interface AlbumTrackTagProps {
  track: FragmentType<typeof albumTrackTagTrackFragment> | undefined | null;
}

const albumTrackTagTrackFragment = graphql(`
  fragment AlbumTrackTag_Track on Track {
    id
    isMissing
    mediaAvailabilityStatus
  }
`);

export const AlbumTrackTag: React.FC<AlbumTrackTagProps> = (props) => {
  const track = useFragment(albumTrackTagTrackFragment, props.track);

  if (!track) {
    return null;
  }

  if (track.mediaAvailabilityStatus === MediaAvailabilityStatus.Downloading) {
    return <Tag>Downloading...</Tag>;
  }

  if (track.mediaAvailabilityStatus === MediaAvailabilityStatus.Processing) {
    return <Tag>Processing...</Tag>;
  }

  if (track.isMissing) {
    return <Tag variant={"error"}>Missing</Tag>;
  }

  return null;
};
