using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Db.Postgres.Models.Events;
using MusicGQL.Db.Postgres.Models.Projections;

namespace MusicGQL.Features.LikedSongs.Aggregate;

public class LikedSongsEventProcessor // No longer using EventProcessor.EventProcessor<T> directly for simplicity here
{
    // Cache for user-specific liked songs projections
    private Dictionary<Guid, LikedSongsProjection> _projectionsCache = new();
    private readonly ILogger<LikedSongsEventProcessor> _logger;

    public LikedSongsEventProcessor(ILogger<LikedSongsEventProcessor> logger)
    {
        _logger = logger;
    }

    public async Task PrepareProcessing(EventDbContext dbContext)
    {
        _logger.LogInformation("Initializing LikedSongsEventProcessor cache...");
        // Pre-load existing projections. This could be memory intensive for many users.
        // Consider alternative strategies if needed (e.g., load on demand in ProcessEvent).
        _projectionsCache = await dbContext.LikedSongsProjections.ToDictionaryAsync(p => p.UserId);
        _logger.LogInformation(
            "LikedSongsEventProcessor cache initialized with {Count} user projections",
            _projectionsCache.Count
        );
    }

    public void ProcessEvent(Event ev, EventDbContext dbContext)
    {
        switch (ev)
        {
            case Db.Postgres.Models.Events.LikedSong likedSongEvent:
                HandleLikedSongEvent(likedSongEvent, dbContext);
                break;
            case UnlikedSong unlikedSongEvent:
                HandleUnlikedSongEvent(unlikedSongEvent, dbContext);
                break;
        }
    }

    private void HandleLikedSongEvent(
        Db.Postgres.Models.Events.LikedSong likedSongEvent,
        EventDbContext dbContext
    )
    {
        if (!_projectionsCache.TryGetValue(likedSongEvent.SubjectUserId, out var projection))
        {
            projection = new LikedSongsProjection
            {
                UserId = likedSongEvent.SubjectUserId,
                LikedSongRecordingIds = new List<string>(),
            };
            dbContext.LikedSongsProjections.Add(projection);
            _projectionsCache[likedSongEvent.SubjectUserId] = projection;
            _logger.LogInformation(
                "Created new LikedSongsProjection for UserId: {UserId}",
                likedSongEvent.SubjectUserId
            );
        }

        if (!projection.LikedSongRecordingIds.Contains(likedSongEvent.RecordingId))
        {
            projection.LikedSongRecordingIds.Add(likedSongEvent.RecordingId);
            projection.LastUpdatedAt = likedSongEvent.CreatedAt; // Use event's timestamp
            _logger.LogInformation(
                "RecordingId {RecordingId} added to liked songs for UserId: {UserId}",
                likedSongEvent.RecordingId,
                likedSongEvent.SubjectUserId
            );
        }
        else
        {
            _logger.LogInformation(
                "RecordingId {RecordingId} already liked by UserId: {UserId}",
                likedSongEvent.RecordingId,
                likedSongEvent.SubjectUserId
            );
        }
    }

    private void HandleUnlikedSongEvent(UnlikedSong unlikedSongEvent, EventDbContext dbContext)
    {
        if (_projectionsCache.TryGetValue(unlikedSongEvent.SubjectUserId, out var projection))
        {
            var removed = projection.LikedSongRecordingIds.RemoveAll(id =>
                id == unlikedSongEvent.RecordingId
            );
            if (removed > 0)
            {
                projection.LastUpdatedAt = unlikedSongEvent.CreatedAt; // Use event's timestamp
                _logger.LogInformation(
                    "RecordingId {RecordingId} removed from liked songs for UserId: {UserId}",
                    unlikedSongEvent.RecordingId,
                    unlikedSongEvent.SubjectUserId
                );
            }
            else
            {
                _logger.LogInformation(
                    "Attempted to unlike RecordingId {RecordingId} for UserId: {UserId}, but it was not liked.",
                    unlikedSongEvent.RecordingId,
                    unlikedSongEvent.SubjectUserId
                );
            }
        }
        else
        {
            _logger.LogWarning(
                "UnlikedSong event for UserId: {UserId} received, but no projection found. RecordingId: {RecordingId}",
                unlikedSongEvent.SubjectUserId,
                unlikedSongEvent.RecordingId
            );
        }
    }
}
