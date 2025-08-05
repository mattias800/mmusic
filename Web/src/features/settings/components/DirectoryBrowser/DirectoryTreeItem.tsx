import { useState } from "react";
import { useQuery } from "urql";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ChevronRight, Lock } from "lucide-react";
import { Button } from "@/components/ui/button";

const directoryTreeItemFileSystemEntryFragment = graphql(`
  fragment DirectoryTreeItem_FileSystemEntry on FileSystemEntry {
    id
    name
    path
    isDirectory
    hasChildren
    isAccessible
  }
`);

const directoryTreeItemQuery = graphql(`
  query DirectoryTreeItem($path: String) {
    browseFileSystem(path: $path) {
      id
      isDirectory
      path
      ...DirectoryTreeItem_FileSystemEntry
    }
  }
`);

export interface DirectoryTreeItemProps {
  entry: FragmentType<typeof directoryTreeItemFileSystemEntryFragment>;
  selectedPath: string | undefined;
  onSelect: (path: string) => void;
  showFiles?: boolean;
}

export function DirectoryTreeItem({
  onSelect,
  selectedPath,
  showFiles,
  ...props
}: DirectoryTreeItemProps) {
  const entry = useFragment(
    directoryTreeItemFileSystemEntryFragment,
    props.entry,
  );

  const [isExpanded, setIsExpanded] = useState(false);

  const [{ data }, executeQuery] = useQuery({
    query: directoryTreeItemQuery,
    variables: { path: entry.path },
    pause: true,
  });

  const handleToggle = () => {
    if (entry.isDirectory && entry.isAccessible) {
      setIsExpanded(!isExpanded);
      if (!data) {
        executeQuery();
      }
    }
  };

  return (
    <div>
      <div className={"flex items-center space-x-2"}>
        <Button
          variant="ghost"
          size="icon"
          onClick={handleToggle}
          disabled={!entry.isDirectory || !entry.isAccessible}
          className={`${!entry.isAccessible ? "text-red-500" : ""}`}
        >
          {entry.isAccessible ? (
            <ChevronRight
              className={`h-4 w-4 transition-transform ${isExpanded ? "rotate-90" : ""}`}
            />
          ) : (
            <Lock className="h-4 w-4" />
          )}
        </Button>

        {(entry.isDirectory || showFiles) && (
          <button
            className={`flex gap-2 cursor-pointer py-1 px-2 rounded-md ${selectedPath === entry.path ? "bg-green-800" : ""}`}
            onClick={() => onSelect(entry.path)}
          >
            <span className={`${!entry.isAccessible ? "text-red-500" : ""}`}>
              {entry.name}
            </span>
          </button>
        )}
      </div>
      {isExpanded && data?.browseFileSystem && (
        <div className="pl-6">
          {data.browseFileSystem
            .filter((f) => f.isDirectory || showFiles)
            .map((entry) => (
              <DirectoryTreeItem
                key={entry.path}
                entry={entry}
                onSelect={onSelect}
                selectedPath={selectedPath}
                showFiles={showFiles}
              />
            ))}
        </div>
      )}
    </div>
  );
}
