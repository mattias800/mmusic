using HotChocolate;

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
    [GraphQLIgnore]
    public string? ArtistName { get; init; }

    [GraphQLIgnore]
    public string? ReleaseTitle { get; init; }

    // Opaque key used for queue management actions (de-duplication and removal)
    public string? QueueKey { get; init; }
}

public record DownloadQueueState
{
    public int QueueLength { get; init; }
    public List<DownloadQueueItem> Items { get; init; } = [];
}

public record DownloadProgress
{
    public string ArtistId { get; init; } = string.Empty;
    public string ReleaseFolderName { get; init; } = string.Empty;
    public DownloadStatus Status { get; init; } = DownloadStatus.Idle;
    public int TotalTracks { get; init; }
    public int CompletedTracks { get; init; }
    public string? ErrorMessage { get; init; }
}


