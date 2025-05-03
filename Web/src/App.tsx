import { ThemeProvider } from "./components/theme-provider.tsx";
import { BrowserRouter } from "react-router";
import { AppRouter } from "@/AppRouter.tsx";
import { PageLayout } from "@/PageLayout.tsx";

function App() {
  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <BrowserRouter>
        <PageLayout>
          <AppRouter />
        </PageLayout>
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;
