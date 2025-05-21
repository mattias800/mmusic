import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import App from './App'; // Assuming App.tsx is in the same directory or adjust path
import SetupPage from './app/Setup'; // Mock or simple version for testing routes

// Mock urql's useQuery
const mockUseQuery = vi.fn();
vi.mock('urql', async (importOriginal) => {
    const actual = await importOriginal() as typeof import('urql');
    return {
        ...actual,
        useQuery: (args: any) => mockUseQuery(args),
        Provider: ({ children }: { children: React.ReactNode }) => <>{children}</>, // Mock UrqlProvider
    };
});

// Mock PageLayout to simplify testing App's core logic
vi.mock('./PageLayout', () => ({
    PageLayout: ({ children }: { children: React.ReactNode }) => <div data-testid="page-layout">{children}</div>,
}));

// Mock MusicPlayer
vi.mock('./features/music-players/MusicPlayer', () => ({
    MusicPlayer: () => <div data-testid="music-player">Music Player</div>,
}));

// Mock ThemeProvider
vi.mock('./components/theme-provider', () => ({
    ThemeProvider: ({ children }: { children: React.ReactNode }) => <div data-testid="theme-provider">{children}</div>,
}));

// Mock Redux Provider and store (if not testing Redux interactions here)
vi.mock('react-redux', () => ({
    Provider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));
vi.mock('./Store', () => ({
    store: {},
}));


// Mock urqlClient (if it's directly used by App, though usually through Provider)
vi.mock('./UrqlClient', () => ({
    urqlClient: {},
}));


// Simple component for home page
const HomePage = () => <div>Home Page</div>;

describe('App.tsx Redirection Logic', () => {
    beforeEach(() => {
        mockUseQuery.mockReset();
    });

    test('redirects to /setup if areThereAnyUsers is false and not on /setup', async () => {
        mockUseQuery.mockReturnValue([{ data: { areThereAnyUsers: false }, fetching: false, error: null }]);
        
        render(
            <MemoryRouter initialEntries={['/']}>
                <App />
            </MemoryRouter>
        );

        // AppSetupLogic shows "Loading..." initially, then triggers navigation
        // We need to wait for the navigation to /setup to be reflected
        await waitFor(() => {
            // Check if SetupPage content is rendered (or a mock of it)
            // For this test, we'll assume SetupPage renders a unique text or test ID
            // If SetupPage is complex, mock it too. Here, we assume it renders "Create Admin User" heading.
             expect(screen.getByText(/create admin user/i)).toBeInTheDocument();
        });
    });

    test('redirects to / if areThereAnyUsers is true and current path is /setup', async () => {
        mockUseQuery.mockReturnValue([{ data: { areThereAnyUsers: true }, fetching: false, error: null }]);
        
        render(
            <MemoryRouter initialEntries={['/setup']}>
                {/* Need to define routes for this test to work correctly with App's structure */}
                <Routes>
                    <Route path="/setup" element={<App />} /> 
                    <Route path="/" element={<HomePage />} />
                </Routes>
            </MemoryRouter>
        );
        
        await waitFor(() => {
             expect(screen.getByText('Home Page')).toBeInTheDocument();
        });
    });

    test('does not redirect if areThereAnyUsers is true and not on /setup', async () => {
        mockUseQuery.mockReturnValue([{ data: { areThereAnyUsers: true }, fetching: false, error: null }]);
        
        render(
            <MemoryRouter initialEntries={['/some-other-page']}>
                 <Routes>
                    <Route path="/some-other-page" element={<App />} />
                    <Route path="/" element={<HomePage />} />
                    <Route path="/setup" element={<SetupPage />} /> {/* Ensure setup route exists for App's logic */}
                </Routes>
            </MemoryRouter>
        );

        // AppSetupLogic will render its children if users exist and not on /setup
        // We expect PageLayout to be rendered, which contains AppRouter, which would render Home (or a 404 if route not matched)
        // For simplicity, check that "Loading..." is gone and no navigation to /setup or / occurs unexpectedly.
        await waitFor(() => {
            expect(screen.queryByText(/loading\.\.\./i)).not.toBeInTheDocument();
        });
        // Check that we are still on "some-other-page" effectively by checking App's structure
        // For example, if AppRouter renders a specific component for /some-other-page or a default one.
        // Here, we assume App's structure remains and doesn't force a redirect.
        // A more specific check would depend on how AppRouter handles unknown routes.
        // We can assert that a known part of the standard app layout (e.g. PageLayout) is present.
        expect(screen.getByTestId('page-layout')).toBeInTheDocument();
        expect(screen.queryByText('Home Page')).not.toBeInTheDocument(); // Shouldn't redirect to home
        expect(screen.queryByText(/create admin user/i)).not.toBeInTheDocument(); // Shouldn't redirect to setup

    });

    test('shows loading state initially', () => {
        mockUseQuery.mockReturnValue([{ data: null, fetching: true, error: null }]);
        render(
            <MemoryRouter initialEntries={['/']}>
                <App />
            </MemoryRouter>
        );
        expect(screen.getByText(/loading\.\.\./i)).toBeInTheDocument();
    });

    test('shows error state if query fails', async () => {
        mockUseQuery.mockReturnValue([{ data: null, fetching: false, error: { message: 'Network Error' } }]);
        render(
            <MemoryRouter initialEntries={['/']}>
                <App />
            </MemoryRouter>
        );
        await waitFor(() => {
            expect(screen.getByText(/error checking user status: network error/i)).toBeInTheDocument();
        });
    });
});
