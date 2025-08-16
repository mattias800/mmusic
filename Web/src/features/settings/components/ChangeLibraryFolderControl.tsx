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
import { AlertTriangle, CheckCircle2, FolderOpen, Settings } from "lucide-react";
import { Alert } from "@/components/ui/Alert.tsx";
import { StatusCard, GradientButton } from "@/components/ui";

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
    <div className="space-y-6">
      {/* Current Library Path Status */}
      <div className="p-4 bg-white/5 rounded-lg border border-white/10">
        <div className="flex items-center gap-3 mb-4">
          <div className="p-2 bg-blue-500/20 rounded-lg">
            <FolderOpen className="w-5 h-5 text-blue-400" />
          </div>
          <h3 className="text-lg font-semibold text-white">Current Library Path</h3>
        </div>
        
        <div className="flex items-center gap-3 mb-4">
          <div className="flex-1 p-3 bg-white/5 rounded-lg border border-white/10">
            <span className="text-gray-300">
              {serverSettings.libraryPath || "(not set)"}
            </span>
          </div>
          {selectedFolderHasManifest ? (
            <div className="flex items-center gap-2 text-emerald-400">
              <CheckCircle2 className="h-5 w-5" />
              <span className="text-sm">Valid</span>
            </div>
          ) : (
            <div className="flex items-center gap-2 text-amber-400">
              <AlertTriangle className="h-5 w-5" />
              <span className="text-sm">No Manifest</span>
            </div>
          )}
        </div>
        
        <GradientButton
          variant="primary"
          onClick={() => setIsConfirmOpen(true)}
          disabled={updateLibraryPathFetching}
          className="w-full"
        >
          Change Library Path
        </GradientButton>
      </div>

      {/* Manifest Warning */}
      {serverSettings.libraryPath && !selectedFolderHasManifest && (
        <div className="p-4 bg-gradient-to-r from-amber-500/10 to-orange-500/10 border border-amber-500/20 rounded-xl">
          <div className="flex items-start gap-3">
            <div className="flex-shrink-0">
              <div className="p-2 bg-amber-500/20 rounded-lg">
                <AlertTriangle className="w-5 h-5 text-amber-400" />
              </div>
            </div>
            <div className="flex-1 min-w-0">
              <h3 className="text-sm font-medium text-amber-200">No Library Manifest Found</h3>
              <p className="mt-1 text-sm text-amber-300">
                The configured folder does not contain a mmusic library manifest.
                Writing is disabled until a manifest is created, to prevent
                accidental writing to the wrong folder, since mmusic downloads media
                automatically.
              </p>
              <div className="mt-3">
                <GradientButton
                  variant="danger"
                  disabled={createManifestInCurrentFolderFetching}
                  onClick={async () => {
                    await createManifestInCurrentFolder({});
                  }}
                  size="sm"
                >
                  Create Manifest in This Folder
                </GradientButton>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Change Library Path Confirmation Dialog */}
      <Dialog open={isConfirmOpen} onOpenChange={setIsConfirmOpen}>
        <DialogContent className="bg-gray-800 border-gray-700">
          <DialogHeader>
            <DialogTitle className="text-white">Change Library Folder?</DialogTitle>
            <DialogDescription className="text-gray-300">
              Changing the library path is a dangerous operation. Make sure the
              new folder contains your existing media, or the library may appear
              empty until rescanned.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button 
              variant="outline" 
              onClick={() => setIsConfirmOpen(false)}
              className="border-gray-600 text-gray-300 hover:bg-gray-700"
            >
              Cancel
            </Button>
            <GradientButton
              variant="danger"
              onClick={() => {
                setIsConfirmOpen(false);
                setIsModalOpen(true);
              }}
            >
              I Understand, Continue
            </GradientButton>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Directory Browser Modal */}
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
        heading={"Select Library Folder"}
      />

      {/* Create Manifest Confirmation Dialog */}
      <Dialog
        open={isCreateManifestOpen}
        onOpenChange={setIsCreateManifestOpen}
      >
        <DialogContent className="bg-gray-800 border-gray-700">
          <DialogHeader>
            <DialogTitle className="text-white">No Manifest Found in Folder</DialogTitle>
            <DialogDescription className="text-gray-300">
              The selected folder does not contain a mmusic library manifest. To
              enable safe writes here, a manifest will be created. Are you sure
              this is the correct folder?
            </DialogDescription>
          </DialogHeader>
          {createError && (
            <div className="text-sm text-red-400 bg-red-500/10 p-3 rounded-lg border border-red-500/20">
              {createError}
            </div>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => {
                setIsCreateManifestOpen(false);
                setPendingPath(null);
                setCreateError(null);
              }}
              className="border-gray-600 text-gray-300 hover:bg-gray-700"
            >
              Cancel
            </Button>
            <GradientButton
              variant="danger"
              onClick={onClickCreateManifestAndSetFolder}
            >
              Create Manifest and Set Folder
            </GradientButton>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
};
