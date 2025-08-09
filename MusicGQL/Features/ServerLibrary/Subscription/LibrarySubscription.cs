using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.ServerLibrary.Subscription;

[ExtendObjectType(typeof(Types.Subscription))]
public record LibrarySubscription
{
    public static string LibraryCacheTrackUpdatedTopic(
        string artistId,
        string releaseFolderName,
        int trackNumber
    ) => $"LibraryCacheTrackUpdated_{artistId}_{releaseFolderName}_{trackNumber}";

    public static string LibraryCacheInTracksReleaseUpdatedTopic(
        string artistId,
        string releaseFolderName
    ) => $"LibraryCacheTrackUpdated_{artistId}_{releaseFolderName}";

    // New: release-level download status updates
    public static string LibraryReleaseDownloadStatusUpdatedTopic(
        string artistId,
        string releaseFolderName
    ) => $"LibraryReleaseDownloadStatusUpdated_{artistId}_{releaseFolderName}";

    public ValueTask<ISourceStream<LibraryCacheTrackStatus>> SubscribeToLibraryCacheTrackUpdated(
        [Service] ITopicEventReceiver receiver,
        string artistId,
        string releaseFolderName,
        int trackNumber,
        CancellationToken cancellationToken
    ) =>
        receiver.SubscribeAsync<LibraryCacheTrackStatus>(
            LibraryCacheTrackUpdatedTopic(artistId, releaseFolderName, trackNumber),
            cancellationToken
        );

    [Subscribe(With = nameof(SubscribeToLibraryCacheTrackUpdated))]
    public LibraryCacheTrackStatus LibraryCacheTrackUpdated(
        [EventMessage] LibraryCacheTrackStatus status
    ) => status;

    public ValueTask<
        ISourceStream<LibraryCacheTrackStatus>
    > SubscribeToLibraryCacheTracksInReleaseUpdated(
        [Service] ITopicEventReceiver receiver,
        string artistId,
        string releaseFolderName,
        CancellationToken cancellationToken
    ) =>
        receiver.SubscribeAsync<LibraryCacheTrackStatus>(
            LibraryCacheInTracksReleaseUpdatedTopic(artistId, releaseFolderName),
            cancellationToken
        );

    [Subscribe(With = nameof(SubscribeToLibraryCacheTracksInReleaseUpdated))]
    public LibraryCacheTrackStatus LibraryCacheTracksInReleaseUpdated(
        [EventMessage] LibraryCacheTrackStatus status
    ) => status;

    // Release-level status subscription
    public ValueTask<
        ISourceStream<LibraryReleaseDownloadStatusUpdate>
    > SubscribeToLibraryReleaseDownloadStatusUpdated(
        [Service] ITopicEventReceiver receiver,
        string artistId,
        string releaseFolderName,
        CancellationToken cancellationToken
    ) =>
        receiver.SubscribeAsync<LibraryReleaseDownloadStatusUpdate>(
            LibraryReleaseDownloadStatusUpdatedTopic(artistId, releaseFolderName),
            cancellationToken
        );

    [Subscribe(With = nameof(SubscribeToLibraryReleaseDownloadStatusUpdated))]
    public LibraryReleaseDownloadStatusUpdate LibraryReleaseDownloadStatusUpdated(
        [EventMessage] LibraryReleaseDownloadStatusUpdate update
    ) => update;
}

public record LibraryCacheTrackStatus(string ArtistId, string ReleaseFolderName, int TrackNumber)
{
    public async Task<Track?> Track(ServerLibraryCache cache)
    {
        var cachedTrack = await cache.GetTrackByArtistReleaseAndNumberAsync(
            ArtistId,
            ReleaseFolderName,
            TrackNumber
        );

        return cachedTrack == null ? null : new(cachedTrack);
    }
}

public record LibraryReleaseDownloadStatusUpdate(
    string ArtistId,
    string ReleaseFolderName,
    ReleaseDownloadStatus Status
);
