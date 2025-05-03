import { ThemeProvider } from "./components/theme-provider.tsx";
import { BrowserRouter } from "react-router";
import { AppRouter } from "@/AppRouter.tsx";
import { PageLayout } from "@/PageLayout.tsx";
import { Provider as UrqlProvider } from "urql";
import { urqlClient } from "@/UrqlClient.ts";

function App() {
  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <UrqlProvider value={urqlClient}>
        <BrowserRouter>
          <PageLayout>
            <AppRouter />
          </PageLayout>
        </BrowserRouter>
      </UrqlProvider>
    </ThemeProvider>
  );
}

export default App;
