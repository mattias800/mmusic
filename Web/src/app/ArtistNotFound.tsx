import * as React from "react";
import { PageNotFound } from "@/components/ui";
import { Button } from "@/components/ui/button";
import { Home, Search } from "lucide-react";
import { Link } from "react-router";

export interface ArtistNotFoundProps {}

export const ArtistNotFound: React.FC<ArtistNotFoundProps> = () => {
  return (
    <PageNotFound
      title="Artist Not Found"
      message="The artist you're looking for doesn't exist in our database or may have been removed."
      actionButton={
        <div className="flex flex-col sm:flex-row gap-3 justify-center">
          <Link to="/">
            <Button
              iconLeft={Home}
              className="bg-gradient-to-r from-blue-500 to-indigo-500 hover:from-blue-600 hover:to-indigo-600 text-white px-6 py-3 rounded-xl font-semibold"
            >
              Go Home
            </Button>
          </Link>
          <Link to="/search">
            <Button
              iconLeft={Search}
              variant="outline"
              className="border-white/20 text-white hover:bg-white/10 px-6 py-3 rounded-xl font-semibold"
            >
              Search Artists
            </Button>
          </Link>
        </div>
      }
    />
  );
};
