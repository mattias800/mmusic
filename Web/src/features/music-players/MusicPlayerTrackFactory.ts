import { FragmentType, graphql, useFragment } from "@/gql";
import { MusicPlayerTrack } from "@/features/music-players/MusicPlayerSlice.ts";

const musicPlayerTrackFactoryTrackFragment = graphql(`
  fragment MusicPlayerTrackFactory_Track on Track {
    id
    title
    trackLength
    trackNumber
    media {
      id
      audioQualityLabel
    }
    release {
      id
      folderName
      coverArtUrl
      artist {
        id
        name
      }
    }
    trackNumber
  }
`);

export const createMusicPlayerTrack = (
  track: FragmentType<typeof musicPlayerTrackFactoryTrackFragment>,
): MusicPlayerTrack => {
  // eslint-disable-next-line react-hooks/rules-of-hooks
  const t = useFragment(musicPlayerTrackFactoryTrackFragment, track);
  return {
    artistId: t.release.artist.id,
    releaseFolderName: t.release.folderName,
    trackNumber: t.trackNumber,
    artistName: t.release.artist.id,
    coverArtUrl: t.release.coverArtUrl,
    title: t.title,
    trackLengthMs: t.trackLength ?? 0,
    qualityLabel: t.media?.audioQualityLabel,
  };
};
