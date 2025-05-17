import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export type MusicPlayer =
  | "youtube-video-id"
  | "youtube-video-search"
  | "recording";

export interface MusicPlayerState {
  isOpen: boolean;
  currentMusicPlayer: MusicPlayer | undefined;
  youtubeVideoId: string | undefined;
  youtubeVideoSearchText: string | undefined;
  recordingId: string | undefined;
}

const initialState: MusicPlayerState = {
  isOpen: false,
  currentMusicPlayer: undefined,
  youtubeVideoId: undefined,
  youtubeVideoSearchText: undefined,
  recordingId: undefined,
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
    openYoutubeVideoId: (
      state,
      action: PayloadAction<{ youtubeVideoId: string }>,
    ) => {
      state.youtubeVideoId = action.payload.youtubeVideoId;
      state.youtubeVideoSearchText = undefined;
      state.recordingId = undefined;
      state.isOpen = true;
      state.currentMusicPlayer = "youtube-video-id";
    },
    openYoutubeVideoSearchText: (
      state,
      action: PayloadAction<{ searchText: string }>,
    ) => {
      state.youtubeVideoId = undefined;
      state.youtubeVideoSearchText = action.payload.searchText;
      state.recordingId = undefined;
      state.isOpen = true;
      state.currentMusicPlayer = "youtube-video-search";
    },
    openRecordingId: (
      state,
      action: PayloadAction<{ recordingId: string }>,
    ) => {
      state.youtubeVideoId = undefined;
      state.youtubeVideoSearchText = undefined;
      state.recordingId = action.payload.recordingId;
      state.isOpen = true;
      state.currentMusicPlayer = "recording";
    },
  },
});
