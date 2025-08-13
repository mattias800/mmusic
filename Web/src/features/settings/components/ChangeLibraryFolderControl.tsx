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
import { FragmentType, graphql, useFragment } from "@/gql";
import { useClient, useMutation } from "urql";
import { Label } from "@/components/ui/label.tsx";
import { AlertTriangle, CheckCircle2 } from "lucide-react";
import { Alert } from "@/components/ui/Alert.tsx";

export interface ChangeLibraryFolderControlProps {
  serverSettings: FragmentType<
    typeof changeLibraryFolderControlServerSettingsFragment
  >;
}

const changeLibraryFolderControlServerSettingsFragment = graphql(`
  fragment ChangeLibraryFolderControl_ServerSettings on ServerSettings {
    libraryPath
    serverLibraryManifestStatus {
      id
      hasLibraryManifest
    }
  }
`);

const hasLibraryManifestQuery = graphql(`
  query ChangeFolder_HasLibraryManifest($path: String!) {
    fileSystem {
      hasLibraryManifest(path: $path)
    }
  }
`);

const updateLibraryPathMutation = graphql(`
  mutation UpdateLibraryPath($newLibraryPath: String!) {
    updateLibraryPath(input: { newLibraryPath: $newLibraryPath }) {
      ... on UpdateLibraryPathSuccess {
        serverSettings {
          id
          libraryPath
        }
      }
    }
  }
`);

const createLibraryManifestMutation = graphql(`
  mutation CreateLibraryManifest {
    createLibraryManifest {
      ... on CreateLibraryManifestSuccess {
        serverLibraryManifestStatus {
          id
          hasLibraryManifest
        }
      }
      ... on CreateLibraryManifestError {
        message
      }
    }
  }
`);

export const ChangeLibraryFolderControl: React.FC<
  ChangeLibraryFolderControlProps
> = (props) => {
  const serverSettings = useFragment(
    changeLibraryFolderControlServerSettingsFragment,
    props.serverSettings,
  );

  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isCreateManifestOpen, setIsCreateManifestOpen] = useState(false);
  const [pendingPath, setPendingPath] = useState<string | null>(null);
  const [createError, setCreateError] = useState<string | null>(null);

  const [{ fetching: updateLibraryPathFetching }, updateLibraryPath] =
    useMutation(updateLibraryPathMutation);

  const client = useClient();

  const [
    { fetching: createManifestInCurrentFolderFetching },
    createManifestInCurrentFolder,
  ] = useMutation(createLibraryManifestMutation);

  const selectedFolderHasManifest =
    serverSettings.serverLibraryManifestStatus.hasLibraryManifest;

  const onClickCreateManifestAndSetFolder = async () => {
    if (!pendingPath) return;
    setCreateError(null);
    await updateLibraryPath({ newLibraryPath: pendingPath });
    const result = await createManifestInCurrentFolder({});
    const r = result.data?.createLibraryManifest;

    switch (r?.__typename) {
      case "CreateLibraryManifestError":
        setCreateError(r.message);
        return;
      case "CreateLibraryManifestSuccess":
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
                {serverSettings.libraryPath || "(not set)"}
              </span>
            </div>
            {selectedFolderHasManifest ? (
              <CheckCircle2 className="h-6 w-6 text-emerald-400" />
            ) : (
              <AlertTriangle className="h-6 w-6 text-amber-400" />
            )}
          </div>
        </div>
        <div>
          <Button
            variant="destructive"
            loading={updateLibraryPathFetching}
            disabled={updateLibraryPathFetching}
            onClick={() => setIsConfirmOpen(true)}
          >
            Change...
          </Button>
        </div>
      </div>

      {serverSettings.libraryPath && !selectedFolderHasManifest && (
        <div className="max-w-lg">
          <Alert variant="warning" title="No library manifest found">
            The configured folder does not contain a mmusic library manifest.
            Writing is disabled until a manifest is created, to prevent
            accidental writing to the wrong folder, since mmusic downloads media
            automatically.
            <div className="mt-2">
              <Button
                disabled={createManifestInCurrentFolderFetching}
                loading={createManifestInCurrentFolderFetching}
                variant="destructive"
                onClick={async () => {
                  await createManifestInCurrentFolder({});
                }}
              >
                Create manifest in this folder
              </Button>
            </div>
          </Alert>
        </div>
      )}

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
              await updateLibraryPath({ newLibraryPath: path });
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
            <Button
              variant="destructive"
              onClick={onClickCreateManifestAndSetFolder}
            >
              Create manifest and set folder
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
};
