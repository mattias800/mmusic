using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using MusicGQL.Features.Artists;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.ServerLibrary.Subscription;

[ExtendObjectType(typeof(Types.Subscription))]
public record LibrarySubscription
{
    // Central artist-level updates
    public static string LibraryArtistUpdatedTopic(string artistId) =>
        $"LibraryArtistUpdated_{artistId}";

    // Central release-level updates
    public static string LibraryReleaseUpdatedTopic(string artistId, string releaseFolderName) =>
        $"LibraryReleaseUpdated_{artistId}_{releaseFolderName}";

    // Central per-artist broadcast for any release under that artist
    public static string LibraryArtistReleaseUpdatedTopic(string artistId) =>
        $"LibraryArtistReleaseUpdated_{artistId}";

    // Central track-level updates
    public static string LibraryTrackUpdatedTopic(
        string artistId,
        string releaseFolderName,
        int trackNumber
    ) => $"LibraryTrackUpdated_{artistId}_{releaseFolderName}_{trackNumber}";

    // Central per-artist broadcast for any track under that artist
    public static string LibraryArtistTrackUpdatedTopic(string artistId) =>
        $"LibraryArtistTrackUpdated_{artistId}";

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

    // Release metadata updated subscription
    public static string LibraryReleaseMetadataUpdatedTopic(
        string artistId,
        string releaseFolderName
    ) => $"LibraryReleaseMetadataUpdated_{artistId}_{releaseFolderName}";

    public ValueTask<ISourceStream<Release>> SubscribeToLibraryReleaseMetadataUpdated(
        [Service] ITopicEventReceiver receiver,
        string artistId,
        string releaseFolderName,
        CancellationToken cancellationToken
    ) =>
        receiver.SubscribeAsync<Release>(
            LibraryReleaseMetadataUpdatedTopic(artistId, releaseFolderName),
            cancellationToken
        );

    [Subscribe(With = nameof(SubscribeToLibraryReleaseMetadataUpdated))]
    public Release LibraryReleaseMetadataUpdated([EventMessage] Release release) => release;

    // Centralized: artist updated
    public ValueTask<ISourceStream<Artist>> SubscribeToLibraryArtistUpdated(
        [Service] ITopicEventReceiver receiver,
        string artistId,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<Artist>(LibraryArtistUpdatedTopic(artistId), cancellationToken);

    [Subscribe(With = nameof(SubscribeToLibraryArtistUpdated))]
    public Artist LibraryArtistUpdated([EventMessage] Artist artist) => artist;

    // Centralized: release updated (metadata or other fields) scoped to release
    public ValueTask<ISourceStream<Release>> SubscribeToLibraryReleaseUpdated(
        [Service] ITopicEventReceiver receiver,
        string artistId,
        string releaseFolderName,
        CancellationToken cancellationToken
    ) =>
        receiver.SubscribeAsync<Release>(
            LibraryReleaseUpdatedTopic(artistId, releaseFolderName),
            cancellationToken
        );

    [Subscribe(With = nameof(SubscribeToLibraryReleaseUpdated))]
    public Release LibraryReleaseUpdated([EventMessage] Release release) => release;

    // Centralized: any release updated under an artist
    public ValueTask<ISourceStream<Release>> SubscribeToLibraryArtistReleaseUpdated(
        [Service] ITopicEventReceiver receiver,
        string artistId,
        CancellationToken cancellationToken
    ) =>
        receiver.SubscribeAsync<Release>(
            LibraryArtistReleaseUpdatedTopic(artistId),
            cancellationToken
        );

    [Subscribe(With = nameof(SubscribeToLibraryArtistReleaseUpdated))]
    public Release LibraryArtistReleaseUpdated([EventMessage] Release release) => release;

    // Centralized: track updated (e.g., media availability)
    public ValueTask<ISourceStream<Track>> SubscribeToLibraryTrackUpdated(
        [Service] ITopicEventReceiver receiver,
        string artistId,
        string releaseFolderName,
        int trackNumber,
        CancellationToken cancellationToken
    ) =>
        receiver.SubscribeAsync<Track>(
            LibraryTrackUpdatedTopic(artistId, releaseFolderName, trackNumber),
            cancellationToken
        );

    [Subscribe(With = nameof(SubscribeToLibraryTrackUpdated))]
    public Track LibraryTrackUpdated([EventMessage] Track track) => track;

    // Centralized: any track updated under an artist
    public ValueTask<ISourceStream<Track>> SubscribeToLibraryArtistTrackUpdated(
        [Service] ITopicEventReceiver receiver,
        string artistId,
        CancellationToken cancellationToken
    ) =>
        receiver.SubscribeAsync<Track>(LibraryArtistTrackUpdatedTopic(artistId), cancellationToken);

    [Subscribe(With = nameof(SubscribeToLibraryArtistTrackUpdated))]
    public Track LibraryArtistTrackUpdated([EventMessage] Track track) => track;
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

public record LibraryReleaseDownloadStatusUpdate(Release Release);
