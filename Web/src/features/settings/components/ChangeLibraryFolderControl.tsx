import * as React from "react";
import { useState } from "react";
import { Button } from "@/components/ui/button.tsx";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog.tsx";
import { DirectoryBrowserModal } from "../components/DirectoryBrowser/DirectoryBrowserModal.tsx";
import { graphql } from "@/gql";
import { useClient, useMutation } from "urql";
import { Label } from "@/components/ui/label.tsx";
import { CheckCircle2 } from "lucide-react";

export interface ChangeLibraryFolderControlProps {
  currentPath: string | undefined | null;
  onPathChanged: (newPath: string) => Promise<void> | void;
  loading?: boolean;
}

const hasLibraryManifestQuery = graphql(`
  query ChangeFolder_HasLibraryManifest($path: String!) {
    fileSystem {
      hasLibraryManifest(path: $path)
    }
  }
`);

const createLibraryManifestMutation = graphql(`
  mutation ChangeFolder_CreateLibraryManifest($path: String!) {
    createLibraryManifest(input: { libraryPath: $path }) {
      ... on CreateLibraryManifestSuccess {
        created
      }
      ... on CreateLibraryManifestError {
        message
      }
    }
  }
`);

export const ChangeLibraryFolderControl: React.FC<
  ChangeLibraryFolderControlProps
> = ({ currentPath, onPathChanged, loading }) => {
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isCreateManifestOpen, setIsCreateManifestOpen] = useState(false);
  const [pendingPath, setPendingPath] = useState<string | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);
  const client = useClient();
  const [, createManifest] = useMutation(createLibraryManifestMutation);

  const onClick = async () => {
    if (!pendingPath) return;
    setCreateError(null);
    const result = await createManifest({ path: pendingPath });
    const r = result.data?.createLibraryManifest;

    switch (r?.__typename) {
      case "CreateLibraryManifestError":
        setCreateError(r.message);
        return;
      case "CreateLibraryManifestSuccess":
        await onPathChanged(pendingPath);
        setIsCreateManifestOpen(false);
        setPendingPath(null);
    }
  };
  return (
    <>
      <div className={"flex flex-col gap-4"}>
        <div className={"flex flex-col gap-2"}>
          <Label style={{ marginBottom: "6px", display: "block" }}>
            Library path
          </Label>

          <div className={"flex items-center gap-2"}>
            <div className=" flex gap-4 border border-zinc-700 rounded-md py-2 px-4">
              <span className={"text-zinc-400"}>
                {currentPath || "(not set)"}
              </span>
            </div>
            <CheckCircle2 className="h-6 w-6 text-green-500" />
          </div>
        </div>
        <div>
          <Button
            variant="destructive"
            loading={!!loading}
            disabled={!!loading}
            onClick={() => setIsConfirmOpen(true)}
          >
            Change...
          </Button>
        </div>
      </div>

      <Dialog open={isConfirmOpen} onOpenChange={setIsConfirmOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Change library folder?</DialogTitle>
            <DialogDescription>
              Changing the library path is a dangerous operation. Make sure the
              new folder contains your existing media, or the library may appear
              empty until rescanned.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsConfirmOpen(false)}>
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={() => {
                setIsConfirmOpen(false);
                setIsModalOpen(true);
              }}
            >
              I understand, continue
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <DirectoryBrowserModal
        open={isModalOpen}
        onOpenChange={setIsModalOpen}
        onSelect={async (path) => {
          setPendingPath(path);
          try {
            const res = await client
              .query(hasLibraryManifestQuery, { path })
              .toPromise();
            const has = res.data?.fileSystem?.hasLibraryManifest;
            if (has) {
              await onPathChanged(path);
              setPendingPath(null);
            } else {
              setIsCreateManifestOpen(true);
            }
          } catch {
            setIsCreateManifestOpen(true);
          }
        }}
        heading={"Select library folder"}
      />

      <Dialog
        open={isCreateManifestOpen}
        onOpenChange={setIsCreateManifestOpen}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>No manifest found in folder</DialogTitle>
            <DialogDescription>
              The selected folder does not contain a mmusic library manifest. To
              enable safe writes here, a manifest will be created. Are you sure
              this is the correct folder?
            </DialogDescription>
          </DialogHeader>
          {createError && (
            <div className="text-sm text-red-400">{createError}</div>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setIsCreateManifestOpen(false);
                setPendingPath(null);
                setCreateError(null);
              }}
            >
              Cancel
            </Button>
            <Button variant="destructive" onClick={onClick}>
              Create manifest and set folder
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
};
