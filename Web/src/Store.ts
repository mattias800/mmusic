import { configureStore } from "@reduxjs/toolkit";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";

export const store = configureStore({
  reducer: {
    musicPlayers: musicPlayerSlice.reducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
