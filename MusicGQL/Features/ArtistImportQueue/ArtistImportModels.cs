using HotChocolate;

namespace MusicGQL.Features.ArtistImportQueue;

public record ArtistImportQueueItem(string ArtistName, string? SongTitle)
{
    public string Id => QueueKey ?? $"{ArtistName}|{SongTitle}";
    [GraphQLIgnore]
    public string? ExternalArtistId { get; init; }

    // If known, prefer importing by MusicBrainz artist id without searching
    [GraphQLIgnore]
    public string? MusicBrainzArtistId { get; init; }

    // Optional context to update a specific playlist item after import completes
    [GraphQLIgnore]
    public string? PlaylistId { get; init; }

    [GraphQLIgnore]
    public string? PlaylistItemId { get; init; }

    // New: job kind to support both initial import and release refresh tasks
    public ArtistImportJobKind JobKind { get; init; } = ArtistImportJobKind.ImportArtist;

    // New: for release refresh jobs
    public string? LocalArtistId { get; init; }

    public string? ReleaseFolderName { get; init; }

    // Opaque key used for queue management actions (de-duplication and removal)
    public string? QueueKey { get; init; }
}

public record ArtistImportQueueState
{
    public string Id { get; init; } = "artistImportQueue";
    public int QueueLength { get; init; }
    public List<ArtistImportQueueItem> Items { get; init; } = [];
}

public enum ArtistImportStatus
{
    Idle,
    ResolvingArtist,
    ImportingArtist,
    ImportingReleases,
    Completed,
    Failed
}

public enum ArtistImportJobKind
{
    ImportArtist,
    RefreshReleaseMetadata,
}

public record ArtistImportProgress
{
    public string Id { get; init; } = "artistImportCurrent";
    [GraphQLIgnore] public string? MusicBrainzArtistId { get; init; }

    public string ArtistName { get; init; } = string.Empty;
    public string? SongTitle { get; init; }
    public ArtistImportStatus Status { get; init; } = ArtistImportStatus.Idle;
    public int TotalReleases { get; init; }
    public int CompletedReleases { get; init; }
    public string? ErrorMessage { get; init; }

    // Human-friendly wrapper for Status for UI consumption
    public ArtistImportStatusInfo StatusInfo => ArtistImportStatusInfo.From(Status);
}

public record ArtistImportStatusInfo
{
    public required ArtistImportStatus Id { get; init; }
    public required string Text { get; init; }

    public static ArtistImportStatusInfo From(ArtistImportStatus status)
    {
        return new ArtistImportStatusInfo
        {
            Id = status,
            Text = status switch
            {
                ArtistImportStatus.Idle => "Idle",
                ArtistImportStatus.ResolvingArtist => "Resolving artist…",
                ArtistImportStatus.ImportingArtist => "Importing artist…",
                ArtistImportStatus.ImportingReleases => "Importing releases…",
                ArtistImportStatus.Completed => "Completed",
                ArtistImportStatus.Failed => "Failed",
                _ => status.ToString()
            }
        };
    }
}