import { useState } from "react";
import { useQuery } from "urql";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ChevronRight, File, Folder, Lock } from "lucide-react";
import { Button } from "@/components/ui/button";
import { BrowseFileSystemQuery } from "@/gql/graphql";

const directoryTreeItemFileSystemEntryFragment = graphql(`
  fragment DirectoryTreeItem_FileSystemEntry on FileSystemEntry {
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
      ...DirectoryTreeItem_FileSystemEntry
    }
  }
`);

export interface Props {
  entry: FragmentType<typeof directoryTreeItemFileSystemEntryFragment>;
  onSelect: (path: string) => void;
}

export function DirectoryTreeItem({ onSelect, ...props }: Props) {
  const entry = useFragment(
    directoryTreeItemFileSystemEntryFragment,
    props.entry,
  );

  const [isExpanded, setIsExpanded] = useState(false);

  const [{data}, executeQuery] = useQuery<BrowseFileSystemQuery>({
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
      <div className="flex items-center space-x-2">
        <Button
          variant="ghost"
          size="icon"
          onClick={handleToggle}
          disabled={!entry.isDirectory || !entry.isAccessible}
          className={!entry.isAccessible ? "text-red-500" : ""}
        >
          {entry.isAccessible ? (
            <ChevronRight
              className={`h-4 w-4 transition-transform ${isExpanded ? "rotate-90" : ""}`}
            />
          ) : (
            <Lock className="h-4 w-4" />
          )}
        </Button>
        {entry.isDirectory ? (
          <Folder className="h-4 w-4" />
        ) : (
          <File className="h-4 w-4" />
        )}
        <span
          className={`cursor-pointer ${!entry.isAccessible ? "text-red-500" : ""}`}
          onClick={() => onSelect(entry.path)}
        >
          {entry.name}
        </span>
      </div>
      {isExpanded && data?.browseFileSystem && (
        <div className="pl-6">
          {data.browseFileSystem.map((child) => {
            return (
              <DirectoryTreeItem
                key={child.path}
                entry={child}
                onSelect={onSelect}
              />
            );
          })}
        </div>
      )}
    </div>
  );
}
