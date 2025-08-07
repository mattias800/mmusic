import { useQuery } from "urql";
import { graphql } from "@/gql";
import { DirectoryTreeItem } from "./DirectoryTreeItem";

const browseFileSystemQuery = graphql(`
  query BrowseFileSystem($path: String) {
    fileSystem {
      browseFileSystem(path: $path) {
        path
        isDirectory
        ...DirectoryTreeItem_FileSystemEntry
      }
    }
  }
`);

export interface DirectoryTreeViewProps {
  onSelect: (path: string) => void;
  selectedPath: string | undefined;
  showFiles?: boolean;
}

export function DirectoryTreeView({
  onSelect,
  selectedPath,
  showFiles,
}: DirectoryTreeViewProps) {
  const [result] = useQuery({
    query: browseFileSystemQuery,
    variables: { path: null },
  });

  const { data, fetching, error } = result;

  if (fetching) return <p>Loading...</p>;
  if (error) return <p>Oh no... {error.message}</p>;

  return (
    <div className="flex flex-col space-y-1">
      {data?.fileSystem.browseFileSystem
        ?.filter((f) => f.isDirectory || showFiles)
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
  );
}
