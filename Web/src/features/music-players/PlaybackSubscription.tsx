import * as React from "react";
import { graphql } from "@/gql";
import { useSubscription } from "urql";
// import { useAppDispatch } from "@/ReduxAppHooks.ts";
// import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";

const playbackSub = graphql(`
  subscription OnPlaybackTriggered {
    onPlaybackTriggered {
      clientId
      playback { artistId releaseFolderName trackNumber trackTitle }
    }
  }
`);

export const PlaybackSubscription: React.FC = () => {
  // const dispatch = useAppDispatch();
  const clientId = getClientId();
  useSubscription({ query: playbackSub }, (_prev, msg) => {
    try {
      const cmd = msg?.onPlaybackTriggered;
      if (!cmd) return msg;
      if (cmd.clientId !== clientId) return msg;
      const p = cmd.playback;
      if (!p?.artistId || !p.releaseFolderName || !p.trackNumber) return null;
      // For now, no direct play by identity exists in slice; noop or implement mapping here in future.
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


