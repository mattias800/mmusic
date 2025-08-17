import * as React from "react";
import { PageLayout, PageHeader, GlassCard, InfoSection } from "@/components/ui";
import {
  PageLoading,
  PageError,
  PageNoArtists,
  PageNoAlbums,
  PageNoPlaylists,
  PageNoDownloads,
  PageNoSearchResults,
  PageNotFound,
} from "./page-states";
import { Button } from "./button";
import { Music, Users, Heart, Download, Search, Home } from "lucide-react";

export const PageStatesDemo: React.FC = () => {
  const [currentState, setCurrentState] = React.useState<string>("main");

  const renderState = () => {
    switch (currentState) {
      case "loading":
        return (
          <PageLoading
            title="Loading Your Music Library"
            subtitle="Fetching artists, albums, and playlists"
            icon={Music}
            iconBgColor="bg-blue-500/20"
            size="lg"
          />
        );
      case "error":
        return (
          <PageError
            title="Connection Failed"
            message="We couldn't connect to your music library"
            error="Network timeout after 30 seconds"
            icon={Music}
            iconBgColor="bg-red-500/20"
            onRetry={() => setCurrentState("main")}
            retryText="Try Again"
          />
        );
      case "no-artists":
        return (
          <PageNoArtists
            actionButton={
              <Button
                iconLeft={Users}
                className="bg-gradient-to-r from-blue-500 to-indigo-500 hover:from-blue-600 hover:to-indigo-600 text-white px-6 py-3 rounded-xl font-semibold"
                onClick={() => setCurrentState("main")}
              >
                Import Your First Artist
              </Button>
            }
          />
        );
      case "no-albums":
        return (
          <PageNoAlbums
            actionButton={
              <Button
                iconLeft={Music}
                className="bg-gradient-to-r from-purple-500 to-pink-500 hover:from-purple-600 hover:to-pink-600 text-white px-6 py-3 rounded-xl font-semibold"
                onClick={() => setCurrentState("main")}
              >
                Start Building Collection
              </Button>
            }
          />
        );
      case "no-playlists":
        return (
          <PageNoPlaylists
            actionButton={
              <Button
                iconLeft={Heart}
                className="bg-gradient-to-r from-pink-500 to-rose-500 hover:from-pink-600 hover:to-rose-600 text-white px-6 py-3 rounded-xl font-semibold"
                onClick={() => setCurrentState("main")}
              >
                Create Your First Playlist
              </Button>
            }
          />
        );
      case "no-downloads":
        return (
          <PageNoDownloads
            actionButton={
              <Button
                iconLeft={Download}
                className="bg-gradient-to-r from-green-500 to-emerald-500 hover:from-green-600 hover:to-emerald-600 text-white px-6 py-3 rounded-xl font-semibold"
                onClick={() => setCurrentState("main")}
              >
                Add to Download Queue
              </Button>
            }
          />
        );
      case "no-search-results":
        return (
          <PageNoSearchResults
            searchTerm="nonexistent artist"
            actionButton={
              <Button
                iconLeft={Search}
                className="bg-gradient-to-r from-blue-500 to-indigo-500 hover:from-blue-600 hover:to-indigo-600 text-white px-6 py-3 rounded-xl font-semibold"
                onClick={() => setCurrentState("main")}
              >
                Try Different Search
              </Button>
            }
          />
        );
      case "not-found":
        return (
          <PageNotFound
            title="Page Not Found"
            message="The page you're looking for doesn't exist in our application."
            actionButton={
              <div className="flex flex-col sm:flex-row gap-3 justify-center">
                <Button
                  iconLeft={Home}
                  className="bg-gradient-to-r from-blue-500 to-indigo-500 hover:from-blue-600 hover:to-indigo-600 text-white px-6 py-3 rounded-xl font-semibold"
                  onClick={() => setCurrentState("main")}
                >
                  Go Home
                </Button>
                <Button
                  iconLeft={Search}
                  variant="outline"
                  className="border-white/20 text-white hover:bg-white/10 px-6 py-3 rounded-xl font-semibold"
                  onClick={() => setCurrentState("main")}
                >
                  Search
                </Button>
              </div>
            }
          />
        );
      default:
        return (
          <PageLayout>
            <PageHeader
              icon={Music}
              title="Page State Components Demo"
              subtitle="Beautiful, consistent loading, error, and no-data states for all pages"
            />

            <div className="max-w-6xl mx-auto space-y-8">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {/* Loading States */}
                <GlassCard
                  title="Loading States"
                  icon={Music}
                  iconBgColor="bg-blue-500/20"
                >
                  <div className="space-y-3">
                    <p className="text-gray-300 text-sm">
                      Beautiful loading states with customizable icons, titles, and sizes
                    </p>
                    <Button
                      onClick={() => setCurrentState("loading")}
                      className="w-full bg-gradient-to-r from-blue-500 to-indigo-500 hover:from-blue-600 hover:to-indigo-600"
                    >
                      View Loading State
                    </Button>
                  </div>
                </GlassCard>

                {/* Error States */}
                <GlassCard
                  title="Error States"
                  icon={Music}
                  iconBgColor="bg-red-500/20"
                >
                  <div className="space-y-3">
                    <p className="text-gray-300 text-sm">
                      Professional error handling with retry functionality
                    </p>
                    <Button
                      onClick={() => setCurrentState("error")}
                      className="w-full bg-gradient-to-r from-red-500 to-pink-500 hover:from-red-600 hover:to-pink-600"
                    >
                      View Error State
                    </Button>
                  </div>
                </GlassCard>

                {/* No Data States */}
                <GlassCard
                  title="No Data States"
                  icon={Music}
                  iconBgColor="bg-yellow-500/20"
                >
                  <div className="space-y-3">
                    <p className="text-gray-300 text-sm">
                      Contextual empty states with helpful messaging
                    </p>
                    <Button
                      onClick={() => setCurrentState("no-artists")}
                      className="w-full bg-gradient-to-r from-yellow-500 to-orange-500 hover:from-yellow-600 hover:to-orange-600"
                    >
                      View No Artists
                    </Button>
                  </div>
                </GlassCard>

                {/* Specialized States */}
                <GlassCard
                  title="Specialized States"
                  icon={Music}
                  iconBgColor="bg-purple-500/20"
                >
                  <div className="space-y-3">
                    <p className="text-gray-300 text-sm">
                      Pre-built states for common scenarios
                    </p>
                    <div className="grid grid-cols-2 gap-2">
                      <Button
                        onClick={() => setCurrentState("no-albums")}
                        size="sm"
                        className="bg-gradient-to-r from-purple-500 to-pink-500 hover:from-purple-600 hover:to-pink-600"
                      >
                        No Albums
                      </Button>
                      <Button
                        onClick={() => setCurrentState("no-playlists")}
                        size="sm"
                        className="bg-gradient-to-r from-pink-500 to-rose-500 hover:from-pink-600 hover:to-rose-600"
                      >
                        No Playlists
                      </Button>
                      <Button
                        onClick={() => setCurrentState("no-downloads")}
                        size="sm"
                        className="bg-gradient-to-r from-green-500 to-emerald-500 hover:from-green-600 hover:to-emerald-600"
                      >
                        No Downloads
                      </Button>
                      <Button
                        onClick={() => setCurrentState("no-search-results")}
                        size="sm"
                        className="bg-gradient-to-r from-blue-500 to-indigo-500 hover:from-blue-600 hover:to-indigo-600"
                      >
                        No Results
                      </Button>
                    </div>
                  </div>
                </GlassCard>

                {/* Not Found */}
                <GlassCard
                  title="Not Found"
                  icon={Search}
                  iconBgColor="bg-blue-500/20"
                >
                  <div className="space-y-3">
                    <p className="text-gray-300 text-sm">
                      Beautiful 404 and not found states
                    </p>
                    <Button
                      onClick={() => setCurrentState("not-found")}
                      className="w-full bg-gradient-to-r from-blue-500 to-indigo-500 hover:from-blue-600 hover:to-indigo-600"
                    >
                      View Not Found
                    </Button>
                  </div>
                </GlassCard>

                {/* Back to Main */}
                <GlassCard
                  title="Navigation"
                  icon={Home}
                  iconBgColor="bg-gray-500/20"
                >
                  <div className="space-y-3">
                    <p className="text-gray-300 text-sm">
                      Return to the main demo view
                    </p>
                    <Button
                      onClick={() => setCurrentState("main")}
                      className="w-full bg-gradient-to-r from-gray-500 to-slate-500 hover:from-gray-600 hover:to-slate-600"
                    >
                      Back to Demo
                    </Button>
                  </div>
                </GlassCard>
              </div>

              <InfoSection
                icon={Music}
                title="About Page State Components"
                variant="purple"
              >
                These beautiful, consistent page state components provide a unified user experience across your entire application. 
                They handle loading states, error conditions, and empty data scenarios with professional styling and helpful messaging. 
                Each component is customizable with icons, colors, and action buttons to fit your specific use case.
              </InfoSection>
            </div>
          </PageLayout>
        );
    }
  };

  return renderState();
};
