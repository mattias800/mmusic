import { useQuery } from "urql";
import { graphql } from "@/gql";
import { DirectoryTreeItem } from "./DirectoryTreeItem";

const browseFileSystemQuery = graphql(`
  query BrowseFileSystem($path: String) {
    browseFileSystem(path: $path) {
      path
      ...DirectoryTreeItem_FileSystemEntry
    }
  }
`);

export interface Props {
  onSelect: (path: string) => void;
}

export function DirectoryTreeView({ onSelect }: Props) {
  const [result] = useQuery({
    query: browseFileSystemQuery,
    variables: { path: null },
  });

  const { data, fetching, error } = result;

  if (fetching) return <p>Loading...</p>;
  if (error) return <p>Oh no... {error.message}</p>;

  return (
    <div className="flex flex-col space-y-1">
      {data?.browseFileSystem?.map((entry) => {
        return (
          <DirectoryTreeItem
            key={entry.path}
            entry={entry}
            onSelect={onSelect}
          />
        );
      })}
    </div>
  );
}
