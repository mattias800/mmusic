import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface MusicPlayerState {
  isOpen: boolean;
  currentMusicPlayer: "library" | undefined;
  artistId?: string;
  releaseFolderName?: string;
  trackNumber?: number;
  // queue
  queue: Array<{
    artistId: string;
    releaseFolderName: string;
    trackNumber: number;
    title?: string;
    artistName?: string;
    coverArtUrl?: string;
  }>;
  currentIndex: number; // index in queue
  isPlaying: boolean;
  volume: number; // 0..1
  muted: boolean;
  positionSec: number; // current time
  durationSec: number; // duration
}

const initialState: MusicPlayerState = {
  isOpen: false,
  currentMusicPlayer: undefined,
  artistId: undefined,
  releaseFolderName: undefined,
  trackNumber: undefined,
  queue: [],
  currentIndex: -1,
  isPlaying: false,
  volume: 1,
  muted: false,
  positionSec: 0,
  durationSec: 0,
};

export const musicPlayerSlice = createSlice({
  name: "musicPlayer",
  initialState,
  reducers: {
    open: (state) => {
      state.isOpen = true;
    },
    close: (state) => {
      state.isOpen = false;
    },
    playTrack: (
      state,
      action: PayloadAction<{
        artistId: string;
        releaseFolderName: string;
        trackNumber: number;
      }>,
    ) => {
      state.artistId = action.payload.artistId;
      state.releaseFolderName = action.payload.releaseFolderName;
      state.trackNumber = action.payload.trackNumber;
      state.isOpen = true;
      state.currentMusicPlayer = "library";
      state.isPlaying = true;
    },
    enqueueAndPlay: (
      state,
      action: PayloadAction<MusicPlayerState["queue"]>,
    ) => {
      state.queue = action.payload;
      state.currentIndex = action.payload.length > 0 ? 0 : -1;
      const current =
        state.currentIndex >= 0
          ? action.payload[state.currentIndex]
          : undefined;
      state.artistId = current?.artistId;
      state.releaseFolderName = current?.releaseFolderName;
      state.trackNumber = current?.trackNumber;
      state.isOpen = state.currentIndex >= 0;
      state.currentMusicPlayer = state.isOpen ? "library" : undefined;
      state.isPlaying = state.isOpen;
      state.positionSec = 0;
    },
    next: (state) => {
      if (state.queue.length === 0) return;
      const nextIndex = state.currentIndex + 1;
      if (nextIndex >= state.queue.length) {
        state.isPlaying = false;
        return;
      }
      state.currentIndex = nextIndex;
      const current = state.queue[state.currentIndex];
      state.artistId = current.artistId;
      state.releaseFolderName = current.releaseFolderName;
      state.trackNumber = current.trackNumber;
      state.isPlaying = true;
      state.positionSec = 0;
    },
    prev: (state) => {
      if (state.queue.length === 0) return;
      const prevIndex = Math.max(0, state.currentIndex - 1);
      state.currentIndex = prevIndex;
      const current = state.queue[state.currentIndex];
      state.artistId = current.artistId;
      state.releaseFolderName = current.releaseFolderName;
      state.trackNumber = current.trackNumber;
      state.isPlaying = true;
      state.positionSec = 0;
    },
    play: (state) => {
      state.isPlaying = true;
    },
    pause: (state) => {
      state.isPlaying = false;
    },
    setVolume: (state, action: PayloadAction<number>) => {
      state.volume = Math.max(0, Math.min(1, action.payload));
    },
    setMuted: (state, action: PayloadAction<boolean>) => {
      state.muted = action.payload;
    },
    seekTo: (state, action: PayloadAction<number>) => {
      state.positionSec = Math.max(0, action.payload);
    },
    updatePlaybackTime: (
      state,
      action: PayloadAction<{ positionSec: number; durationSec: number }>,
    ) => {
      state.positionSec = action.payload.positionSec;
      state.durationSec = action.payload.durationSec;
    },
  },
});
