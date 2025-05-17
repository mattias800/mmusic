import { ThemeProvider } from "./components/theme-provider.tsx";
import { BrowserRouter } from "react-router";
import { AppRouter } from "@/AppRouter.tsx";
import { PageLayout } from "@/PageLayout.tsx";
import { Provider as UrqlProvider } from "urql";
import { Provider as ReduxProvider } from "react-redux";
import { urqlClient } from "@/UrqlClient.ts";
import { store } from "@/Store.ts";
import { MusicPlayer } from "@/features/music-players/MusicPlayer.tsx";

function App() {
  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <ReduxProvider store={store}>
        <UrqlProvider value={urqlClient}>
          <MusicPlayer />
          <BrowserRouter>
            <PageLayout>
              <AppRouter />
            </PageLayout>
          </BrowserRouter>
        </UrqlProvider>
      </ReduxProvider>
    </ThemeProvider>
  );
}

export default App;
