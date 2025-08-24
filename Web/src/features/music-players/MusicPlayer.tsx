import * as React from "react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { RootState } from "@/Store.ts";
import { useAppDispatch, useAppSelector } from "@/ReduxAppHooks.ts";
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
  ListMusic,
  Pause,
  Play,
  SkipBack,
  SkipForward,
  Volume2,
  VolumeX,
} from "lucide-react";
import { Tag } from "@/components/text/Tag.tsx";
import {
  castPause,
  castPlay,
  castSeek,
  castSetVolume,
  loadMediaOnCast,
} from "@/features/casting/cast-sender.ts";
import { useCast } from "@/features/casting/useCast.ts";
import { useServerPublicBaseUrl } from "@/features/casting/useServerPublicBaseUrl.ts";
import { CastButton } from "@/features/casting/CastButton.tsx";
import { getCastAbsoluteUrl } from "@/features/casting/cast-url.ts";

export interface MusicPlayerProps {}

const selector = (state: RootState) => state.musicPlayers;

export const MusicPlayer: React.FC<MusicPlayerProps> = () => {
  const dispatch = useAppDispatch();
  const audioRef = useRef<HTMLAudioElement | null>(null);

  const {
    isOpen,
    currentTrack,
    queue,
    history,
    currentIndex,
    isPlaying,
    volume,
    muted,
  } = useAppSelector(selector);
  const [queueOpen, setQueueOpen] = useState(false);
  const [scrubValue, setScrubValue] = useState<number | null>(null);
  const [positionSec, setPositionSec] = useState(0);
  const [durationSec, setDurationSec] = useState(0);
  const { isReady: castReady, hasSession: castSession } = useCast();
  const publicBaseUrl = useServerPublicBaseUrl();

  const commitScrub = useCallback(() => {
    if (scrubValue != null) {
      setPositionSec(scrubValue);
      if (castSession) castSeek(scrubValue);
      else if (audioRef.current) audioRef.current.currentTime = scrubValue;
    }
    setScrubValue(null);
  }, [scrubValue, castSession]);

  const src = useMemo(
    () =>
      currentTrack == null
        ? undefined
        : `/library/${encodeURIComponent(currentTrack.artistId)}/releases/${encodeURIComponent(
            currentTrack.releaseFolderName,
          )}/tracks/${currentTrack.trackNumber}/audio`,
    [currentTrack],
  );

  // Cast initialization handled by useCast

  useEffect(() => {
    if (castSession) {
      castSetVolume(volume, muted);
    } else if (audioRef.current) {
      audioRef.current.volume = volume;
      audioRef.current.muted = muted;
    }
  }, [volume, muted, castSession]);

  useEffect(() => {
    if (castSession) {
      if (isPlaying) castPlay();
      else castPause();
    } else if (audioRef.current) {
      if (isPlaying) audioRef.current.play().catch(() => {});
      else audioRef.current.pause();
    }
  }, [isPlaying, src, castSession]);

  useEffect(() => {
    if (!castSession || !src || !currentTrack) return;
    const absoluteUrl = getCastAbsoluteUrl(src, publicBaseUrl);
    loadMediaOnCast({
      contentUrl: absoluteUrl,
      title: currentTrack.title,
      imageUrl: currentTrack.coverArtUrl,
      contentType: "audio/mpeg",
      autoplay: isPlaying,
      startTime: positionSec,
    }).catch(() => {});
    // We do not depend on positionSec continuously to avoid reloading
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [castSession, src, currentTrack]);

  if (!isOpen) {
    return null;
  }

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
                {currentTrack?.title ??
                  `${currentTrack?.artistId ?? ""} - ${currentTrack?.releaseFolderName ?? ""}`}
              </div>
              {currentTrack?.qualityLabel && (
                <div className="text-xs text-muted-foreground opacity-70 pr-8">
                  <Tag>{currentTrack.qualityLabel}</Tag>
                </div>
              )}
            </div>
            <div className="truncate text-muted-foreground text-sm">
              {currentTrack?.artistName ?? currentTrack?.artistId}
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
            {castReady && <CastButton className="h-10 w-10" />}
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
              value={scrubValue ?? positionSec}
              onChange={(e) => setScrubValue(parseFloat(e.target.value))}
              onPointerUp={commitScrub}
              onBlur={commitScrub}
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

      {!castSession && (
        <audio
          ref={audioRef}
          src={src}
          autoPlay
          onDurationChange={() => {
            setDurationSec(audioRef.current?.duration ?? 0);
          }}
          onTimeUpdate={() => {
            setPositionSec(audioRef.current?.currentTime ?? 0);
          }}
          onEnded={() => dispatch(musicPlayerSlice.actions.next())}
          style={{ width: "100%" }}
        />
      )}
    </div>
  );
};
