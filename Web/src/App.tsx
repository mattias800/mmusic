import { BrowserRouter } from "react-router";
import { AppRouter } from "@/AppRouter.tsx";
import { GlobalPageLayout } from "@/GlobalPageLayout.tsx";
import { Provider as UrqlProvider } from "urql";
import { Provider as ReduxProvider } from "react-redux";
import { urqlClient } from "@/UrqlClient.ts";
import { store } from "@/Store.ts";
import { MusicPlayer } from "@/features/music-players/MusicPlayer.tsx";
import { Bootstrap } from "@/Bootstrap.tsx";
import { SetupPage } from "@/app/SetupPage.tsx";
import { SignInPage } from "@/app/SignInPage.tsx";
import { SidebarSwitcher } from "@/features/sidebar/SidebarSwitcher.tsx";
import { ThemeProvider } from "@/components/theme-provider.tsx";
import { SearchInput } from "@/features/search/search-input/SearchInput.tsx";
import { PlaybackSubscription } from "@/features/music-players/PlaybackSubscription.tsx";

function App() {
  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <ReduxProvider store={store}>
        <UrqlProvider value={urqlClient}>
          <Bootstrap
            renderNoUsers={() => <SetupPage />}
            renderAuthenticated={() => (
              <>
                <MusicPlayer />
                <BrowserRouter>
                  <GlobalPageLayout
                    renderSidebar={() => <SidebarSwitcher />}
                    renderSearch={() => <SearchInput />}
                  >
                    <AppRouter />
                    <PlaybackSubscription />
                  </GlobalPageLayout>
                </BrowserRouter>
              </>
            )}
            renderNotAuthenticated={() => <SignInPage />}
          />
        </UrqlProvider>
      </ReduxProvider>
    </ThemeProvider>
  );
}

export default App;
