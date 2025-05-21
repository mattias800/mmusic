import { ThemeProvider } from "./components/theme-provider.tsx";
import { BrowserRouter } from "react-router";
import { AppRouter } from "@/AppRouter.tsx";
import { PageLayout } from "@/PageLayout.tsx";
import { Provider as UrqlProvider } from "urql";
import { Provider as ReduxProvider } from "react-redux";
import { urqlClient } from "@/UrqlClient.ts";
import { store } from "@/Store.ts";
import { MusicPlayer } from "@/features/music-players/MusicPlayer.tsx";
import React, { useEffect } from 'react';
import { useQuery } from 'urql';
import { useNavigate, useLocation } from 'react-router-dom';
import { ARE_THERE_ANY_USERS_QUERY } from '@/gql/misc';

// Component to handle initial setup logic
const AppSetupLogic: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const [{ data, fetching, error }] = useQuery({ query: ARE_THERE_ANY_USERS_QUERY });

  useEffect(() => {
    if (!fetching && data) {
      if (data.areThereAnyUsers === false && location.pathname !== '/setup') {
        navigate('/setup', { replace: true });
      } else if (data.areThereAnyUsers === true && location.pathname === '/setup') {
        // If users exist and user is on /setup, redirect to home
        navigate('/', { replace: true });
      }
    }
  }, [data, fetching, navigate, location.pathname]);

  if (fetching) {
    // You might want to return a loading spinner here
    return <div>Loading...</div>;
  }

  if (error) {
    // Handle error state, perhaps show an error message
    return <div>Error checking user status: {error.message}</div>;
  }
  
  // If users exist or we are on the setup page, render children
  if (data && (data.areThereAnyUsers === true || location.pathname === '/setup')) {
    return <>{children}</>;
  }
  
  // Fallback for initial load or if data.areThereAnyUsers is null/undefined briefly
  // before navigation to /setup happens.
  return <div>Loading...</div>; 
};

function App() {
  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <ReduxProvider store={store}>
        <UrqlProvider value={urqlClient}>
          <MusicPlayer />
          <BrowserRouter>
            {/* Wrap PageLayout and AppRouter with AppSetupLogic */}
            <AppSetupLogic>
              <PageLayout>
                <AppRouter />
              </PageLayout>
            </AppSetupLogic>
          </BrowserRouter>
        </UrqlProvider>
      </ReduxProvider>
    </ThemeProvider>
  );
}

export default App;
