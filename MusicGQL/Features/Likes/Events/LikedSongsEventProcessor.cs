using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.Likes.Db;

namespace MusicGQL.Features.Likes.Events;

public class LikedSongsEventProcessor(ILogger<LikedSongsEventProcessor> logger)
{
    public async Task ProcessEvent(Event ev, EventDbContext dbContext)
    {
        switch (ev)
        {
            case LikedSong likedSongEvent:
                await HandleLikedSongEvent(likedSongEvent, dbContext);
                break;
            case UnlikedSong unlikedSongEvent:
                await HandleUnlikedSongEvent(unlikedSongEvent, dbContext);
                break;
        }
    }

    private async Task HandleLikedSongEvent(LikedSong likedSongEvent, EventDbContext dbContext)
    {
        var existingLike = dbContext.LikedSongs.FirstOrDefault(ls =>
            ls.LikedByUserId == likedSongEvent.SubjectUserId
            && ls.RecordingId == likedSongEvent.RecordingId
        );

        if (existingLike == null)
        {
            var newLike = new DbLikedSong
            {
                LikedByUserId = likedSongEvent.SubjectUserId,
                RecordingId = likedSongEvent.RecordingId,
                LikedAt = likedSongEvent.CreatedAt,
            };
            dbContext.LikedSongs.Add(newLike);

            await dbContext.SaveChangesAsync();

            logger.LogInformation(
                "RecordingId {RecordingId} added to liked songs for UserId: {UserId}",
                likedSongEvent.RecordingId,
                likedSongEvent.SubjectUserId
            );
        }
        else
        {
            logger.LogInformation(
                "RecordingId {RecordingId} already liked by UserId: {UserId}. Updating LikedAt time",
                likedSongEvent.RecordingId,
                likedSongEvent.SubjectUserId
            );
        }
    }

    private async Task HandleUnlikedSongEvent(
        UnlikedSong unlikedSongEvent,
        EventDbContext dbContext
    )
    {
        var likeToRemove = dbContext.LikedSongs.FirstOrDefault(ls =>
            ls.LikedByUserId == unlikedSongEvent.SubjectUserId
            && ls.RecordingId == unlikedSongEvent.RecordingId
        );

        if (likeToRemove != null)
        {
            dbContext.LikedSongs.Remove(likeToRemove);
            await dbContext.SaveChangesAsync();
            logger.LogInformation(
                "RecordingId {RecordingId} removed from liked songs for UserId: {UserId}",
                unlikedSongEvent.RecordingId,
                unlikedSongEvent.SubjectUserId
            );
        }
        else
        {
            logger.LogWarning(
                "Attempted to unlike RecordingId {RecordingId} for UserId: {UserId}, but it was not found as liked",
                unlikedSongEvent.RecordingId,
                unlikedSongEvent.SubjectUserId
            );
        }
    }
}
