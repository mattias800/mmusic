using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ArtistImportQueue;
using MusicGQL.Features.Artists;
using MusicGQL.Types;

namespace MusicGQL.Features.Import;

/// <summary>
/// Consolidated subscription class for all import-related operations.
/// Handles both traditional queue-based imports and background imports.
/// </summary>
[ExtendObjectType(typeof(Subscription))]
public record ImportSubscription
{
    // ===== TRADITIONAL QUEUE-BASED IMPORTS =====
    
    /// <summary>
    /// Subscribe to artist import queue updates
    /// </summary>
    public ValueTask<ISourceStream<ArtistImportQueueState>> SubscribeToArtistImportQueueUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<ArtistImportQueueState>("ArtistImportQueueUpdated", cancellationToken);

    [Subscribe(With = nameof(SubscribeToArtistImportQueueUpdated))]
    public ArtistImportQueueState ArtistImportQueueUpdated([EventMessage] ArtistImportQueueState state) => state;

    /// <summary>
    /// Subscribe to current artist import progress updates
    /// </summary>
    public ValueTask<ISourceStream<ArtistImportProgress>> SubscribeToCurrentArtistImportUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<ArtistImportProgress>("CurrentArtistImportUpdated", cancellationToken);

    [Subscribe(With = nameof(SubscribeToCurrentArtistImportUpdated))]
    public ArtistImportProgress CurrentArtistImportUpdated([EventMessage] ArtistImportProgress progress) => progress;

    /// <summary>
    /// Subscribe to artist import completion
    /// </summary>
    public ValueTask<ISourceStream<Artist>> SubscribeToArtistImported(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<Artist>("ArtistImported", cancellationToken);

    [Subscribe(With = nameof(SubscribeToArtistImported))]
    public Artist ArtistImported([EventMessage] Artist artist) => artist;

    // ===== BACKGROUND IMPORTS =====
    
    /// <summary>
    /// Subscribe to background artist import queue updates
    /// </summary>
    public ValueTask<ISourceStream<ArtistImportBackgroundQueueState>> SubscribeToArtistImportBackgroundQueueUpdated(
        [Service] ITopicEventReceiver receiver,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<ArtistImportBackgroundQueueState>("ArtistImportBackgroundQueueUpdated", cancellationToken);

    [Subscribe(With = nameof(SubscribeToArtistImportBackgroundQueueUpdated))]
    public ArtistImportBackgroundQueueState ArtistImportBackgroundQueueUpdated([EventMessage] ArtistImportBackgroundQueueState state) => state;

    /// <summary>
    /// Subscribe to background artist import progress updates for a specific artist
    /// </summary>
    public ValueTask<ISourceStream<ArtistImportBackgroundProgress>> SubscribeToArtistImportBackgroundProgress(
        [Service] ITopicEventReceiver receiver,
        string artistId,
        CancellationToken cancellationToken
    ) => receiver.SubscribeAsync<ArtistImportBackgroundProgress>(ArtistImportBackgroundProgressTopic(artistId), cancellationToken);

    [Subscribe(With = nameof(SubscribeToArtistImportBackgroundProgress))]
    public ArtistImportBackgroundProgress ArtistImportBackgroundProgress(
        string artistId,
        [EventMessage] ArtistImportBackgroundProgress progress
    ) => progress;

    /// <summary>
    /// Builds the topic name for artist-specific background progress updates
    /// </summary>
    public static string ArtistImportBackgroundProgressTopic(string artistId) => $"ArtistImportBackgroundProgress_{artistId}";
}
