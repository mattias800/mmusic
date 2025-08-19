// Minimal Google Cast sender helpers with lightweight types

declare global {
  interface Window {
    __onGCastApiAvailable?: (isAvailable: boolean) => void;
    cast?: {
      framework?: {
        CastContext: {
          getInstance: () => {
            addEventListener: (type: unknown, listener: unknown) => void;
            setOptions: (opts: unknown) => void;
            getCurrentSession: () => {
              getMediaSession?: () => {
                play?: () => void;
                pause?: () => void;
                seek?: (req: unknown) => void;
                setVolume?: (
                  req: unknown,
                  onSuccess?: () => void,
                  onError?: () => void,
                ) => void;
              } | null;
              loadMedia?: (req: unknown) => Promise<void>;
              getSessionObj?: () => {
                loadMedia?: (req: unknown) => Promise<void>;
                setReceiverVolumeLevel?: (
                  level: number,
                  onSuccess?: () => void,
                  onError?: () => void,
                ) => void;
                setReceiverMuted?: (muted: boolean) => void;
              } | null;
            } | null;
            requestSession: () => Promise<void>;
            endCurrentSession: (stopCasting: boolean) => void;
          };
        };
        CastContextEventType?: {
          CAST_STATE_CHANGED: unknown;
          SESSION_STATE_CHANGED: unknown;
        };
      };
    };
    chrome?: {
      cast?: {
        AutoJoinPolicy?: { TAB_AND_ORIGIN_SCOPED: unknown };
        media?: {
          DEFAULT_MEDIA_RECEIVER_APP_ID?: string;
          MediaInfo: new (contentId: string, contentType?: string) => unknown;
          LoadRequest: new (mediaInfo: unknown) => unknown;
          SeekRequest: new () => { currentTime: number };
          VolumeRequest: new () => unknown;
          Volume: new (level?: number, muted?: boolean) => unknown;
          StreamType: Record<string, unknown>;
          MusicTrackMediaMetadata: new () => {
            title?: string;
            images?: Array<unknown>;
          };
        };
        Image: new (url: string) => unknown;
      };
    };
  }
}

export interface CastState {
  isReady: boolean;
  hasSession: boolean;
}

export type CastStateListener = (state: CastState) => void;

let initialized = false;
let listeners: CastStateListener[] = [];

export const getCastState = (): CastState => {
  const cast = window.cast;
  const context = cast?.framework?.CastContext.getInstance?.();
  const hasSession = !!context?.getCurrentSession?.();
  return {
    isReady: Boolean(cast?.framework),
    hasSession,
  };
};

const notify = () => {
  const state = getCastState();
  listeners.forEach((l) => l(state));
};

export const onCastStateChange = (listener: CastStateListener) => {
  listeners.push(listener);
  return () => {
    listeners = listeners.filter((l) => l !== listener);
  };
};

export const ensureCastInitialized = () => {
  if (initialized) return;
  initialized = true;
  const onAvailable = (isAvailable: boolean) => {
    if (!isAvailable || !window.cast?.framework) return;
    const context = window.cast.framework.CastContext.getInstance();
    context.setOptions({
      receiverApplicationId:
        window.chrome?.cast?.media?.DEFAULT_MEDIA_RECEIVER_APP_ID || 'CC1AD845',
      autoJoinPolicy: window.chrome?.cast?.AutoJoinPolicy?.TAB_AND_ORIGIN_SCOPED,
    });
    notify();
    // Subscribe to state changes
    context.addEventListener(
      window.cast.framework.CastContextEventType.CAST_STATE_CHANGED,
      notify,
    );
    context.addEventListener(
      window.cast.framework.CastContextEventType.SESSION_STATE_CHANGED,
      notify,
    );
  };
  if (window.cast?.framework) {
    onAvailable(true);
  } else {
    window.__onGCastApiAvailable = onAvailable;
  }
};

export interface LoadMediaParams {
  contentUrl: string;
  contentType?: string;
  title?: string;
  imageUrl?: string;
  streamType?: 'BUFFERED' | 'LIVE' | 'OTHER';
  autoplay?: boolean;
  startTime?: number;
}

export const loadMediaOnCast = async (params: LoadMediaParams) => {
  const cast = window.cast;
  if (!cast?.framework) throw new Error('Cast not ready');
  const context = cast.framework.CastContext.getInstance();
  const session = context.getCurrentSession();
  if (!session) throw new Error('No cast session');

  const mediaInfo = new window.chrome.cast.media.MediaInfo(
    params.contentUrl,
    params.contentType || 'audio/mpeg',
  );
  if (params.title || params.imageUrl) {
    const md = new window.chrome.cast.media.MusicTrackMediaMetadata();
    if (params.title) (md as { title?: string }).title = params.title;
    if (params.imageUrl)
      (md as { images?: Array<unknown> }).images = [
        new window.chrome.cast.Image(params.imageUrl),
      ];
    (mediaInfo as { metadata?: unknown }).metadata = md;
  }
  mediaInfo.streamType =
    window.chrome.cast.media.StreamType[params.streamType || 'BUFFERED'];

  const request = new window.chrome.cast.media.LoadRequest(mediaInfo);
  request.autoplay = params.autoplay ?? true;
  request.currentTime = params.startTime ?? 0;

  // Some sender SDKs require calling loadMedia via getSessionObj().loadMedia; we try both
  if (typeof session.loadMedia === 'function') {
    await session.loadMedia(request);
  } else {
    const raw = session.getSessionObj?.();
    if (raw && typeof raw.loadMedia === 'function') {
      await raw.loadMedia(request);
    }
  }
};

export const castPlay = () => {
  const context = window.cast?.framework?.CastContext.getInstance?.();
  const media = context?.getCurrentSession?.()?.getMediaSession?.();
  media?.play?.();
};

export const castPause = () => {
  const context = window.cast?.framework?.CastContext.getInstance?.();
  const media = context?.getCurrentSession?.()?.getMediaSession?.();
  media?.pause?.();
};

export const castSeek = (seconds: number) => {
  const context = window.cast?.framework?.CastContext.getInstance?.();
  const media = context?.getCurrentSession?.()?.getMediaSession?.();
  if (!media?.seek) return;
  const req = new window.chrome.cast.media.SeekRequest();
  req.currentTime = seconds;
  media.seek(req);
};

export const castSetVolume = (volume0to1: number, muted: boolean) => {
  const session = window.cast?.framework
    ?.CastContext.getInstance?.()
    ?.getCurrentSession?.();
  const media = session?.getMediaSession?.();
  const receiver = session?.getSessionObj?.();
  if (media?.setVolume) {
    const req = new window.chrome.cast.media.VolumeRequest();
    req.volume = new window.chrome.cast.Volume(volume0to1, muted);
    media.setVolume(req, () => {}, () => {});
  } else if (receiver?.setReceiverVolumeLevel) {
    receiver.setReceiverVolumeLevel(volume0to1, () => {}, () => {});
    if (muted && receiver.setReceiverMuted) receiver.setReceiverMuted(true);
  }
};

export const startCastSession = async () => {
  const cast = window.cast;
  if (!cast?.framework) throw new Error('Cast not ready');
  const context = cast.framework.CastContext.getInstance();
  await context.requestSession();
  notify();
};

export const endCastSession = () => {
  const context = window.cast?.framework?.CastContext.getInstance?.();
  context?.endCurrentSession?.(true);
  notify();
};


