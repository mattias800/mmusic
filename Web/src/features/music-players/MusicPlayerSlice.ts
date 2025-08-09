import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface MusicPlayerTrack {
  artistId: string;
  releaseFolderName: string;
  trackNumber: number;
  title?: string;
  artistName?: string;
  coverArtUrl?: string;
  trackLengthMs?: number;
  qualityLabel?: string;
}

export interface MusicPlayerState {
  isOpen: boolean;
  currentMusicPlayer: "library" | undefined;
  currentTrack?: MusicPlayerTrack;
  // queue
  queue: Array<MusicPlayerTrack>;
  // play history
  history: Array<{
    track: MusicPlayerTrack;
    startedAtIso: string;
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
  currentTrack: undefined,
  queue: [],
  history: [],
  currentIndex: -1,
  isPlaying: false,
  volume: 1,
  muted: false,
  positionSec: 0,
  durationSec: 0,
};

const pushCurrentToHistory = (state: MusicPlayerState) => {
  if (!state.currentTrack) return;
  state.history.unshift({
    track: state.currentTrack,
    startedAtIso: new Date().toISOString(),
  });
  if (state.history.length > 500) state.history.length = 500;
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
      state.currentTrack = {
        artistId: action.payload.artistId,
        releaseFolderName: action.payload.releaseFolderName,
        trackNumber: action.payload.trackNumber,
      };
      state.isOpen = true;
      state.currentMusicPlayer = "library";
      state.isPlaying = true;
      pushCurrentToHistory(state);
    },
    enqueueAndPlay: (
      state,
      action: PayloadAction<MusicPlayerState["queue"]>,
    ) => {
      state.queue = action.payload;
      state.currentIndex = action.payload.length > 0 ? 0 : -1;
      const current = state.currentIndex >= 0 ? action.payload[state.currentIndex] : undefined;
      state.currentTrack = current;
      state.isOpen = state.currentIndex >= 0;
      state.currentMusicPlayer = state.isOpen ? "library" : undefined;
      state.isPlaying = state.isOpen;
      state.positionSec = 0;
      if (state.isOpen) pushCurrentToHistory(state);
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
      state.currentTrack = current;
      state.isPlaying = true;
      state.positionSec = 0;
      pushCurrentToHistory(state);
    },
    prev: (state) => {
      if (state.queue.length === 0) return;
      const prevIndex = Math.max(0, state.currentIndex - 1);
      state.currentIndex = prevIndex;
      const current = state.queue[state.currentIndex];
      state.currentTrack = current;
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
    updateCurrentQualityLabel: (
      state,
      action: PayloadAction<string | undefined>,
    ) => {
      if (state.currentIndex < 0 || state.currentIndex >= state.queue.length) return;
      state.queue[state.currentIndex] = {
        ...state.queue[state.currentIndex],
        qualityLabel: action.payload,
      };
      if (state.currentTrack && state.currentTrack.artistId === state.queue[state.currentIndex].artistId && state.currentTrack.releaseFolderName === state.queue[state.currentIndex].releaseFolderName && state.currentTrack.trackNumber === state.queue[state.currentIndex].trackNumber) {
        state.currentTrack = { ...state.currentTrack, qualityLabel: action.payload };
      }
    },
    playAtIndex: (state, action: PayloadAction<number>) => {
      const idx = action.payload;
      if (idx < 0 || idx >= state.queue.length) return;
      state.currentIndex = idx;
      const current = state.queue[state.currentIndex];
      state.currentTrack = current;
      state.isPlaying = true;
      state.positionSec = 0;
      state.isOpen = true;
      state.currentMusicPlayer = "library";
      pushCurrentToHistory(state);
    },
  },
});
