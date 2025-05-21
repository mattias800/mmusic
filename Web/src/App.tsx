import { BrowserRouter } from "react-router";
import { AppRouter } from "@/AppRouter.tsx";
import { PageLayout } from "@/PageLayout.tsx";
import { Provider as UrqlProvider } from "urql";
import { Provider as ReduxProvider } from "react-redux";
import { urqlClient } from "@/UrqlClient.ts";
import { store } from "@/Store.ts";
import { MusicPlayer } from "@/features/music-players/MusicPlayer.tsx";
import { MantineProvider } from "@mantine/core";
import { Notifications } from "@mantine/notifications";
import { Bootstrap } from "@/Bootstrap.tsx";
import Setup from "@/app/Setup.tsx";

function App() {
  return (
    <MantineProvider defaultColorScheme="dark">
      <ReduxProvider store={store}>
        <UrqlProvider value={urqlClient}>
          <Bootstrap
            renderNoUsers={() => <Setup />}
            renderAuthenticated={() => (
              <>
                <Notifications />
                <MusicPlayer />
                <BrowserRouter>
                  <PageLayout>
                    <AppRouter />
                  </PageLayout>
                </BrowserRouter>
              </>
            )}
            renderNotAuthenticated={() => <span>Logga in då</span>}
          />
        </UrqlProvider>
      </ReduxProvider>
    </MantineProvider>
  );
}

export default App;
