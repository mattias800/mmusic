import * as React from "react";
import {
  LoaderCircle,
  AlertTriangle,
  AlertCircle,
  Music,
  Users,
  Disc3,
  Heart,
  Download,
  Search,
} from "lucide-react";
import { PageLayout } from "./page-layout";

// ============================================================================
// LOADING STATE COMPONENTS
// ============================================================================

export interface PageLoadingProps {
  title?: string;
  subtitle?: string;
  icon?: React.ComponentType<{ className?: string }>;
  iconBgColor?: string;
  size?: "sm" | "md" | "lg";
}

export const PageLoading: React.FC<PageLoadingProps> = ({
  title = "Loading...",
  subtitle = "Please wait while we fetch your data",
  icon: Icon = LoaderCircle,
  iconBgColor = "bg-blue-500/20",
  size = "md",
}) => {
  const sizeClasses = {
    sm: "w-16 h-16",
    md: "w-24 h-24",
    lg: "w-32 h-32",
  };

  const iconSizes = {
    sm: "w-8 h-8",
    md: "w-12 h-12",
    lg: "w-16 h-16",
  };

  return (
    <PageLayout>
      <div className="min-h-[60vh] flex items-center justify-center">
        <div className="text-center space-y-6">
          {/* Loading Icon */}
          <div className="flex justify-center">
            <div
              className={`${sizeClasses[size]} ${iconBgColor} rounded-3xl border border-white/20 flex items-center justify-center`}
            >
              <Icon
                className={`${iconSizes[size]} text-blue-400 animate-pulse`}
              />
            </div>
          </div>

          {/* Loading Text */}
          <div className="space-y-2">
            <h2 className="text-2xl font-bold text-white">{title}</h2>
            <p className="text-gray-300 text-lg">{subtitle}</p>
          </div>

          {/* Animated Dots */}
          <div className="flex justify-center space-x-2">
            <div
              className="w-2 h-2 bg-blue-400 rounded-full animate-bounce"
              style={{ animationDelay: "0ms" }}
            ></div>
            <div
              className="w-2 h-2 bg-blue-400 rounded-full animate-bounce"
              style={{ animationDelay: "150ms" }}
            ></div>
            <div
              className="w-2 h-2 bg-blue-400 rounded-full animate-bounce"
              style={{ animationDelay: "300ms" }}
            ></div>
          </div>
        </div>
      </div>
    </PageLayout>
  );
};

// ============================================================================
// ERROR STATE COMPONENTS
// ============================================================================

export interface PageErrorProps {
  title?: string;
  message?: string;
  error?: Error | string;
  icon?: React.ComponentType<{ className?: string }>;
  iconBgColor?: string;
  onRetry?: () => void;
  retryText?: string;
}

export const PageError: React.FC<PageErrorProps> = ({
  title = "Something went wrong",
  message = "We encountered an error while loading this page",
  error,
  icon: Icon = AlertTriangle,
  iconBgColor = "bg-red-500/20",
  onRetry,
  retryText = "Try Again",
}) => {
  const errorMessage = error instanceof Error ? error.message : error;

  return (
    <PageLayout>
      <div className="min-h-[60vh] flex items-center justify-center">
        <div className="text-center space-y-6 max-w-md mx-auto">
          {/* Error Icon */}
          <div className="flex justify-center">
            <div
              className={`w-24 h-24 ${iconBgColor} rounded-3xl border border-red-500/30 flex items-center justify-center`}
            >
              <Icon className="w-12 h-12 text-red-400" />
            </div>
          </div>

          {/* Error Text */}
          <div className="space-y-3">
            <h2 className="text-2xl font-bold text-white">{title}</h2>
            <p className="text-gray-300 text-lg">{message}</p>

            {errorMessage && (
              <div className="p-4 bg-red-500/10 rounded-xl border border-red-500/20">
                <p className="text-red-400 text-sm font-mono break-words">
                  {errorMessage}
                </p>
              </div>
            )}
          </div>

          {/* Retry Button */}
          {onRetry && (
            <div className="pt-4">
              <button
                onClick={onRetry}
                className="inline-flex items-center gap-3 px-6 py-3 bg-gradient-to-r from-red-500/20 to-pink-500/20 hover:from-red-500/30 hover:to-pink-500/30 border border-red-500/30 hover:border-red-500/50 rounded-xl text-white font-semibold transition-all duration-200"
              >
                <AlertTriangle className="w-5 h-5" />
                {retryText}
              </button>
            </div>
          )}
        </div>
      </div>
    </PageLayout>
  );
};

// ============================================================================
// NO DATA STATE COMPONENTS
// ============================================================================

export interface PageNoDataProps {
  title?: string;
  message?: string;
  icon?: React.ComponentType<{ className?: string }>;
  iconBgColor?: string;
  actionButton?: React.ReactNode;
  variant?: "default" | "empty" | "not-found";
}

export const PageNoData: React.FC<PageNoDataProps> = ({
  title = "No data available",
  message = "There's nothing to display on this page",
  icon: Icon = AlertCircle,
  iconBgColor = "bg-yellow-500/20",
  actionButton,
  variant = "default",
}) => {
  const getVariantStyles = () => {
    switch (variant) {
      case "empty":
        return {
          icon: Music,
          iconBgColor: "bg-purple-500/20",
          iconColor: "text-purple-400",
          borderColor: "border-purple-500/30",
        };
      case "not-found":
        return {
          icon: Search,
          iconBgColor: "bg-blue-500/20",
          iconColor: "text-blue-400",
          borderColor: "border-blue-500/30",
        };
      default:
        return {
          icon: Icon,
          iconBgColor,
          iconColor: "text-yellow-400",
          borderColor: "border-yellow-500/30",
        };
    }
  };

  const variantStyles = getVariantStyles();
  const VariantIcon = variantStyles.icon;

  return (
    <PageLayout>
      <div className="min-h-[60vh] flex items-center justify-center">
        <div className="text-center space-y-6 max-w-md mx-auto">
          {/* No Data Icon */}
          <div className="flex justify-center">
            <div
              className={`w-24 h-24 ${variantStyles.iconBgColor} rounded-3xl border ${variantStyles.borderColor} flex items-center justify-center`}
            >
              <VariantIcon className={`w-12 h-12 ${variantStyles.iconColor}`} />
            </div>
          </div>

          {/* No Data Text */}
          <div className="space-y-3">
            <h2 className="text-2xl font-bold text-white">{title}</h2>
            <p className="text-gray-300 text-lg">{message}</p>
          </div>

          {/* Action Button */}
          {actionButton && <div className="pt-4">{actionButton}</div>}
        </div>
      </div>
    </PageLayout>
  );
};

// ============================================================================
// SPECIALIZED NO DATA COMPONENTS
// ============================================================================

export const PageNoArtists: React.FC<{ actionButton?: React.ReactNode }> = ({
  actionButton,
}) => (
  <PageNoData
    title="No artists in library"
    message="Your music library is empty. Start by importing some artists to build your collection."
    icon={Users}
    iconBgColor="bg-blue-500/20"
    variant="empty"
    actionButton={actionButton}
  />
);

export const PageNoAlbums: React.FC<{ actionButton?: React.ReactNode }> = ({
  actionButton,
}) => (
  <PageNoData
    title="No albums available"
    message="No albums found. Artists need to be imported first to see their releases."
    icon={Disc3}
    iconBgColor="bg-purple-500/20"
    variant="empty"
    actionButton={actionButton}
  />
);

export const PageNoPlaylists: React.FC<{ actionButton?: React.ReactNode }> = ({
  actionButton,
}) => (
  <PageNoData
    title="No playlists found"
    message="You haven't created any playlists yet. Start building your music collections."
    icon={Heart}
    iconBgColor="bg-pink-500/20"
    variant="empty"
    actionButton={actionButton}
  />
);

export const PageNoDownloads: React.FC<{ actionButton?: React.ReactNode }> = ({
  actionButton,
}) => (
  <PageNoData
    title="No downloads in progress"
    message="No downloads are currently running. Add some artists or releases to your download queue."
    icon={Download}
    iconBgColor="bg-green-500/20"
    variant="empty"
    actionButton={actionButton}
  />
);

export const PageNoSearchResults: React.FC<{
  searchTerm?: string;
  actionButton?: React.ReactNode;
}> = ({ searchTerm, actionButton }) => (
  <PageNoData
    title="No search results found"
    message={
      searchTerm
        ? `No results found for "${searchTerm}". Try a different search term.`
        : "No results found for your search."
    }
    icon={Search}
    iconBgColor="bg-blue-500/20"
    variant="not-found"
    actionButton={actionButton}
  />
);

export const PageNotFound: React.FC<{
  title?: string;
  message?: string;
  actionButton?: React.ReactNode;
}> = ({
  title = "Page not found",
  message = "The page you're looking for doesn't exist or has been moved.",
  actionButton,
}) => (
  <PageNoData
    title={title}
    message={message}
    icon={Search}
    iconBgColor="bg-blue-500/20"
    variant="not-found"
    actionButton={actionButton}
  />
);

// ============================================================================
// QUICK ACCESS COMPONENTS
// ============================================================================

export const PageLoadingSimple: React.FC = () => <PageLoading />;
export const PageErrorSimple: React.FC<{ error?: Error | string }> = ({
  error,
}) => <PageError error={error} />;
export const PageNoDataSimple: React.FC = () => <PageNoData />;
