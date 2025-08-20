import * as React from "react";
import { graphql } from "@/gql";
import { useSubscription } from "urql";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";

const playbackSub = graphql(`
  subscription OnPlaybackTriggered {
    onPlaybackTriggered {
      clientId
      playback {
        artistId
        releaseFolderName
        trackNumber
        trackTitle
        track {
          id
          title
          trackNumber
          trackLength
          release {
            id
            folderName
            coverArtUrl
            artist {
              id
              name
            }
          }
          media {
            id
            audioQualityLabel
          }
        }
      }
    }
  }
`);

export const PlaybackSubscription: React.FC = () => {
  const dispatch = useAppDispatch();
  const clientId = getClientId();
  useSubscription({ query: playbackSub }, (_prev, msg) => {
    try {
      const cmd = msg?.onPlaybackTriggered;
      if (!cmd) return msg;
      if (cmd.clientId !== clientId) return msg;
      const allow = getAllowRemotePlayback();
      if (!allow) return msg;
      const p = cmd.playback;
      if (!p?.artistId || !p.releaseFolderName || !p.trackNumber) return msg;
      if (p.track) {
        // Build full track from nested data if available
        dispatch(
          musicPlayerSlice.actions.playTrack({
            artistId: p.track.release.artist.id,
            releaseFolderName: p.track.release.folderName,
            trackNumber: p.track.trackNumber,
            title: p.track.title,
            artistName: p.track.release.artist.name,
            coverArtUrl: p.track.release.coverArtUrl,
            trackLengthMs: p.track.trackLength ?? 0,
            qualityLabel: p.track.media?.audioQualityLabel ?? undefined,
          }),
        );
      } else {
        // Fallback: minimal identity
        dispatch(
          musicPlayerSlice.actions.playTrack({
            artistId: p.artistId!,
            releaseFolderName: p.releaseFolderName!,
            trackNumber: p.trackNumber!,
            title: p.trackTitle ?? "",
            artistName: p.artistId!,
            coverArtUrl: "",
            trackLengthMs: 0,
            qualityLabel: undefined,
          }),
        );
      }
    } catch {
      // ignore
    }
    return msg;
  });
  return null;
};

function getClientId(): string {
  try {
    return localStorage.getItem("mmusic_client_id") || "";
  } catch {
    return "";
  }
}

function getAllowRemotePlayback(): boolean {
  try {
    const v = localStorage.getItem("mmusic_allow_remote_playback");
    if (v == null) return true;
    return v === "true";
  } catch {
    return true;
  }
}
