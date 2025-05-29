import * as React from "react";

export interface NoResultsFoundProps {}

export const NoResultsFound: React.FC<NoResultsFoundProps> = () => {
  return <div className="text-white">No results found</div>;
}; 