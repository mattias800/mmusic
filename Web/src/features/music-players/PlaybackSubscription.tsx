import * as React from "react";
import { graphql } from "@/gql";
import { useSubscription } from "urql";
import { useEffect } from "react";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";

const playbackSub = graphql(`
  subscription OnPlaybackTriggered {
    onPlaybackTriggered {
      clientId
      playback { artistId releaseFolderName trackNumber trackTitle }
    }
  }
`);

export const PlaybackSubscription: React.FC = () => {
  const dispatch = useAppDispatch();
  const clientId = getClientId();
  useSubscription({ query: playbackSub }, (_prev, msg) => {
    try {
      const cmd = msg?.onPlaybackTriggered;
      if (!cmd) return null;
      if (cmd.clientId !== clientId) return null;
      const p = cmd.playback;
      if (!p?.artistId || !p.releaseFolderName || !p.trackNumber) return null;
      dispatch(musicPlayerSlice.actions.playAtIdentity({
        artistId: p.artistId,
        releaseFolderName: p.releaseFolderName,
        trackNumber: p.trackNumber,
      }));
    } catch {}
    return null;
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


