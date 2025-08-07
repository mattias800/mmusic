import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface MusicPlayerState {
  isOpen: boolean;
  currentMusicPlayer: "library" | undefined;
  artistId?: string;
  releaseFolderName?: string;
  trackNumber?: number;
}

const initialState: MusicPlayerState = {
  isOpen: false,
  currentMusicPlayer: undefined,
  artistId: undefined,
  releaseFolderName: undefined,
  trackNumber: undefined,
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
    },
  },
});
