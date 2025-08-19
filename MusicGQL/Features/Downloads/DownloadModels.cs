using HotChocolate;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Downloads;

public enum DownloadStatus
{
    Idle,
    Searching,
    Downloading,
    Processing,
    Completed,
    Failed
}

public record DownloadQueueItem(string ArtistId, string ReleaseFolderName)
{
    public string Id => QueueKey ?? $"{ArtistId}|{ReleaseFolderName}";

    public string? ArtistName { get; init; }

    public string? ReleaseTitle { get; init; }

    // Opaque key used for queue management actions (de-duplication and removal)
    public string? QueueKey { get; init; }
}

public record DownloadQueueState
{
    public string Id { get; init; } = "downloadQueue";
    public int QueueLength { get; init; }
    public List<DownloadQueueItem> Items { get; init; } = [];
}

public record DownloadProgress
{
    public string Id => $"{ArtistId}|{ReleaseFolderName}";
    public string ArtistId { get; init; } = string.Empty;
    public string ReleaseFolderName { get; init; } = string.Empty;
    public string? ArtistName { get; init; }
    public string? ReleaseTitle { get; init; }
    public DownloadStatus Status { get; init; } = DownloadStatus.Idle;
    public int TotalTracks { get; init; }
    public int CompletedTracks { get; init; }
    public string? ErrorMessage { get; init; }

    // Optional rich metadata for better UI
    public string? CoverArtUrl { get; init; }
    public double? CurrentTrackProgressPercent { get; init; }
    public double? CurrentDownloadSpeedKbps { get; init; }

    // Current provider information
    public string? CurrentProvider { get; init; }
    public int CurrentProviderIndex { get; init; }
    public int TotalProviders { get; init; }
}

public record DownloadSlotInfo(
    int Id,
    bool IsActive,
    bool IsWorking,
    DownloadQueueItem? CurrentWork,
    DownloadProgress? CurrentProgress,
    DateTime? StartedAt,
    DateTime? LastActivityAt,
    string? Status
);

public record DownloadHistoryItem(
    DateTime TimestampUtc,
    string ArtistId,
    string ReleaseFolderName,
    string? ArtistName,
    string? ReleaseTitle,
    bool Success,
    string? ErrorMessage,
    string? ProviderUsed
);

/// <summary>
/// Enhanced download history item with granular state and result tracking
/// </summary>
public record EnhancedDownloadHistoryItem
{
    public string Id { get; init; } = string.Empty;
    public DateTime TimestampUtc { get; init; }
    public string ArtistId { get; init; } = string.Empty;
    public string ReleaseFolderName { get; init; } = string.Empty;
    public string? ArtistName { get; init; }
    public string? ReleaseTitle { get; init; }
    public DownloadState CurrentState { get; init; }
    public DownloadResult? FinalResult { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ProviderUsed { get; init; }
    public List<DownloadStateTransition> StateTransitions { get; init; } = new();
    public TimeSpan? TotalDuration { get; init; }
    public DateTime StateStartTime { get; init; }
}

/// <summary>
/// Tracks a single state transition with timing information
/// </summary>
public record DownloadStateTransition
{
    public DownloadState FromState { get; init; }
    public DownloadState ToState { get; init; }
    public DateTime Timestamp { get; init; }
    public TimeSpan DurationInPreviousState { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Current download state for a specific release
/// </summary>
public record CurrentDownloadState
{
    public string ArtistId { get; init; } = string.Empty;
    public string ReleaseFolderName { get; init; } = string.Empty;
    public DownloadState CurrentState { get; init; }
    public DownloadResult? CurrentResult { get; init; }
    public DateTime StateStartTime { get; init; }
    public TimeSpan CurrentStateDuration => DateTime.UtcNow - StateStartTime;
    public List<DownloadStateTransition> StateTransitions { get; init; } = new();
    public string? CurrentProvider { get; init; }
    public int ProviderIndex { get; init; }
    public int TotalProviders { get; init; }
    public string? ErrorMessage { get; init; }
}