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
}

const initialState: MusicPlayerState = {
  isOpen: false,
  currentTrack: undefined,
  queue: [],
  history: [],
  currentIndex: -1,
  isPlaying: false,
  volume: 1,
  muted: false,
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
      state.isPlaying = true;
      pushCurrentToHistory(state);
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
      state.currentTrack = current;
      state.isOpen = state.currentIndex >= 0;
      state.isPlaying = state.isOpen;
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
      pushCurrentToHistory(state);
    },
    prev: (state) => {
      if (state.queue.length === 0) return;
      const prevIndex = Math.max(0, state.currentIndex - 1);
      state.currentIndex = prevIndex;
      const current = state.queue[state.currentIndex];
      state.currentTrack = current;
      state.isPlaying = true;
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
    playAtIndex: (state, action: PayloadAction<number>) => {
      const idx = action.payload;
      if (idx < 0 || idx >= state.queue.length) return;
      state.currentIndex = idx;
      const current = state.queue[state.currentIndex];
      state.currentTrack = current;
      state.isPlaying = true;
      state.isOpen = true;
      pushCurrentToHistory(state);
    },
  },
});
