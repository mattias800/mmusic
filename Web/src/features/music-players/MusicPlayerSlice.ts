import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export type MusicPlayer = "youtube-music";

export interface MusicPlayerState {
  isOpen: boolean;
  currentMusicPlayer: MusicPlayer | undefined;
  youtubeId: string | undefined;
}

const initialState: MusicPlayerState = {
  isOpen: false,
  currentMusicPlayer: undefined,
  youtubeId: undefined,
};

export const musicPlayerSlice = createSlice({
  name: "musicPlayer",
  initialState,
  reducers: {
    open: (state) => {
      state.isOpen = true;
    },
    openYoutubeMusicId: (
      state,
      action: PayloadAction<{ youtubeId: string }>,
    ) => {
      state.youtubeId = action.payload.youtubeId;
      state.isOpen = true;
      state.currentMusicPlayer = "youtube-music";
    },
  },
});
