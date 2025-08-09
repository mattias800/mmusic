import * as React from "react";
import { RootState } from "@/Store.ts";
import { useAppDispatch, useAppSelector } from "@/ReduxAppHooks.ts";
import { LibraryAudioPlayer } from "@/features/music-players/library-audio-player/LibraryAudioPlayer.tsx";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { QueuePanel } from "@/features/music-players/QueuePanel.tsx";
import { Button } from "@/components/ui/button.tsx";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet.tsx";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs.tsx";
import { HistoryPanel } from "@/features/music-players/HistoryPanel.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import {
  Pause,
  Play,
  SkipBack,
  SkipForward,
  Volume2,
  VolumeX,
  ListMusic,
} from "lucide-react";
import { useState } from "react";
import { Tag } from "@/components/text/Tag.tsx";

export interface MusicPlayerProps {}

const selector = (state: RootState) => state.musicPlayers;

export const MusicPlayer: React.FC<MusicPlayerProps> = () => {
  const dispatch = useAppDispatch();
  const [queueOpen, setQueueOpen] = useState(false);
  const {
    currentMusicPlayer,
    isOpen,
    currentTrack,
    queue,
    history,
    currentIndex,
    isPlaying,
    positionSec,
    durationSec,
    volume,
    muted,
  } = useAppSelector(selector);

  if (!isOpen) {
    return null;
  }

  const currentQueueItem =
    currentIndex >= 0 && currentIndex < queue.length
      ? queue[currentIndex]
      : undefined;

  console.log({ currentTrack });

  return (
    <div className="fixed bottom-0 left-0 right-0 bg-background/90 backdrop-blur border-t px-4 py-2 z-50">
      <div className="max-w-screen-2xl mx-auto grid grid-cols-3 items-center gap-4">
        {/* Left: Now playing info */}
        <div className="flex items-center gap-3 min-w-0">
          {currentTrack && (
            <ReleaseCoverArt
              srcUrl={currentTrack.coverArtUrl}
              titleForPlaceholder={currentTrack.title ?? ""}
              className="h-12 w-12 rounded"
            />
          )}
          <div className="min-w-0">
            <div className={"flex gap-4 items-center"}>
              <div className="truncate font-medium">
                {currentQueueItem?.title ??
                  `${currentTrack?.artistId ?? ""} - ${currentTrack?.releaseFolderName ?? ""}`}
              </div>
              {currentTrack?.qualityLabel && (
                <div className="text-xs text-muted-foreground opacity-70 pr-8">
                  <Tag>{currentTrack.qualityLabel}</Tag>
                </div>
              )}
            </div>
            <div className="truncate text-muted-foreground text-sm">
              {currentQueueItem?.artistName ?? currentTrack?.artistId}
            </div>
          </div>
        </div>

        {/* Center: Transport & Scrubber */}
        <div className="flex flex-col items-center gap-1">
          <div className="flex items-center gap-4">
            <Button
              variant="ghost"
              size="icon"
              className="h-10 w-10"
              onClick={() => dispatch(musicPlayerSlice.actions.prev())}
            >
              <SkipBack className="h-5 w-5" />
            </Button>
            {isPlaying ? (
              <Button
                variant="default"
                size="icon"
                className="h-12 w-12 rounded-full"
                onClick={() => dispatch(musicPlayerSlice.actions.pause())}
              >
                <Pause className="h-6 w-6" />
              </Button>
            ) : (
              <Button
                variant="default"
                size="icon"
                className="h-12 w-12 rounded-full"
                onClick={() => dispatch(musicPlayerSlice.actions.play())}
              >
                <Play className="h-6 w-6" />
              </Button>
            )}
            <Button
              variant="ghost"
              size="icon"
              className="h-10 w-10"
              onClick={() => dispatch(musicPlayerSlice.actions.next())}
            >
              <SkipForward className="h-5 w-5" />
            </Button>
          </div>
          <div className="flex items-center gap-2 w-full">
            <span className="text-xs tabular-nums w-10 text-right">
              {formatTrackLength(positionSec * 1000)}
            </span>
            <input
              type="range"
              min={0}
              max={Math.max(1, durationSec)}
              step={0.1}
              value={positionSec}
              onChange={(e) =>
                dispatch(
                  musicPlayerSlice.actions.seekTo(parseFloat(e.target.value)),
                )
              }
              className="w-full"
            />
            <span className="text-xs tabular-nums w-10">
              {formatTrackLength(durationSec * 1000)}
            </span>
          </div>
        </div>

        {/* Right: Volume + Queue */}
        <div className="flex items-center justify-end gap-2">
          <Button
            variant="ghost"
            size="icon"
            className="h-10 w-10"
            onClick={() => setQueueOpen(true)}
            aria-label="Open queue"
            title="Open queue"
          >
            <ListMusic className="h-5 w-5" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="h-10 w-10"
            onClick={() => dispatch(musicPlayerSlice.actions.setMuted(!muted))}
          >
            {muted ? (
              <VolumeX className="h-5 w-5" />
            ) : (
              <Volume2 className="h-5 w-5" />
            )}
          </Button>
          <input
            type="range"
            min={0}
            max={1}
            step={0.01}
            value={muted ? 0 : volume}
            onChange={(e) =>
              dispatch(
                musicPlayerSlice.actions.setVolume(parseFloat(e.target.value)),
              )
            }
            className="w-28 accent-green-600"
          />
        </div>
      </div>

      {/* Queue/History Sheet */}
      <Sheet open={queueOpen} onOpenChange={setQueueOpen}>
        <SheetContent side="right" className="p-0">
          <SheetHeader>
            <SheetTitle>Playback</SheetTitle>
          </SheetHeader>
          <Tabs defaultValue="queue" className="flex-1 overflow-hidden">
            <div className="px-3 pt-2">
              <TabsList>
                <TabsTrigger value="queue">Queue ({queue.length})</TabsTrigger>
                <TabsTrigger value="history">
                  History ({history.length})
                </TabsTrigger>
              </TabsList>
            </div>
            <TabsContent value="queue" className="flex-1 overflow-y-auto">
              <QueuePanel
                queue={queue}
                currentIndex={currentIndex}
                onSelect={(idx) =>
                  dispatch(musicPlayerSlice.actions.playAtIndex(idx))
                }
              />
            </TabsContent>
            <TabsContent value="history" className="flex-1 overflow-y-auto">
              <HistoryPanel history={history} />
            </TabsContent>
          </Tabs>
        </SheetContent>
      </Sheet>

      {currentMusicPlayer === "library" && currentTrack && (
        <LibraryAudioPlayer
          artistId={currentTrack.artistId}
          releaseFolderName={currentTrack.releaseFolderName}
          trackNumber={currentTrack.trackNumber}
        />
      )}
    </div>
  );
};
