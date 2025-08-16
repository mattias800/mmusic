import * as React from "react";
import { Search } from "lucide-react";

export interface NoResultsFoundProps {}

export const NoResultsFound: React.FC<NoResultsFoundProps> = () => {
  return (
    <div className="flex flex-col items-center justify-center py-8 text-center">
      <div className="p-3 bg-gray-500/20 rounded-full border border-gray-500/30 mb-3">
        <Search className="w-5 h-5 text-gray-400" />
      </div>
      <p className="text-gray-300 text-sm font-medium">No results found</p>
      <p className="text-gray-400 text-xs mt-1">Try adjusting your search terms</p>
    </div>
  );
};
