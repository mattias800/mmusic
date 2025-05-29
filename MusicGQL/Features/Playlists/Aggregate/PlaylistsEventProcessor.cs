using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Db.Postgres.Models.Events.Playlists;
using MusicGQL.Db.Postgres.Models.Projections;

namespace MusicGQL.Features.Playlists.Aggregate;

public class PlaylistsEventProcessor(ILogger<PlaylistsEventProcessor> logger)
{
    public Task PrepareProcessing(EventDbContext dbContext)
    {
        return Task.CompletedTask;
    }

    public async Task ProcessEvent(Event ev, EventDbContext dbContext)
    {
        switch (ev)
        {
            case CreatedPlaylist e:
                await HandleEvent(e, dbContext);
                break;

            case RenamedPlaylist e:
                await HandleEvent(e, dbContext);
                break;
            case SongAddedToPlaylist e:
                await HandleEvent(e, dbContext);
                break;
        }
    }

    private async Task HandleEvent(CreatedPlaylist createdPlaylist, EventDbContext dbContext)
    {
        if (!createdPlaylist.ActorUserId.HasValue)
        {
            return;
        }

        var projection = await dbContext.PlaylistsProjections.FindAsync(
            createdPlaylist.ActorUserId.Value
        );

        if (projection is null)
        {
            projection = new PlaylistsProjection
            {
                UserId = createdPlaylist.ActorUserId.Value,
                Playlists = [],
            };
            dbContext.PlaylistsProjections.Add(projection);
        }

        projection.Playlists.Add(
            new PlaylistProjection
            {
                Id = createdPlaylist.PlaylistId,
                Name = createdPlaylist.Name,
                Description = createdPlaylist.Description,
                CoverImageUrl = createdPlaylist.CoverImageUrl,
                CreatedAt = createdPlaylist.CreatedAt,
                ModifiedAt = null,
                RecordingIds = [],
            }
        );
    }

    private async Task HandleEvent(RenamedPlaylist renamePlaylistEvent, EventDbContext dbContext)
    {
        if (!renamePlaylistEvent.ActorUserId.HasValue)
        {
            return;
        }

        var projection = await dbContext
            .PlaylistsProjections.Include(p => p.Playlists)
            .FirstOrDefaultAsync(p =>
                p.Playlists.Any(pl => pl.Id == renamePlaylistEvent.PlaylistId)
            );

        if (projection is null)
        {
            return;
        }

        var index = projection.Playlists.FindIndex(pl => pl.Id == renamePlaylistEvent.PlaylistId);
        if (index < 0)
        {
            return;
        }

        var updated = projection.Playlists[index] with
        {
            Name = renamePlaylistEvent.NewPlaylistName,
        };
        projection.Playlists[index] = updated;

        logger.LogInformation(
            "Renamed Playlist {PlaylistId} to '{NewPlaylistName}' for UserId: {UserId}",
            renamePlaylistEvent.PlaylistId,
            renamePlaylistEvent.NewPlaylistName,
            renamePlaylistEvent.ActorUserId.Value
        );
    }

    private async Task HandleEvent(SongAddedToPlaylist songAddedEvent, EventDbContext dbContext)
    {
        if (!songAddedEvent.ActorUserId.HasValue)
        {
            return;
        }

        var projection = await dbContext
            .PlaylistsProjections.Include(p => p.Playlists)
            .FirstOrDefaultAsync(p => p.Playlists.Any(pl => pl.Id == songAddedEvent.PlaylistId));

        if (projection is null)
        {
            return;
        }

        var index = projection.Playlists.FindIndex(pl => pl.Id == songAddedEvent.PlaylistId);
        if (index < 0)
        {
            return;
        }

        var existing = projection.Playlists[index];

        // Avoid adding duplicate songs
        if (existing.RecordingIds.Contains(songAddedEvent.RecordingId))
        {
            return;
        }

        var newRecordingIds = existing.RecordingIds.ToList();

        if (songAddedEvent.Position is { } pos && pos >= 0 && pos <= newRecordingIds.Count)
        {
            newRecordingIds.Insert(pos, songAddedEvent.RecordingId);
        }
        else
        {
            newRecordingIds.Add(songAddedEvent.RecordingId);
        }

        var updated = existing with
        {
            RecordingIds = newRecordingIds,
            ModifiedAt = DateTime.UtcNow,
        };

        projection.Playlists[index] = updated;

        logger.LogInformation(
            "Added recording {RecordingId} to Playlist {PlaylistId} at position {Position} for UserId: {UserId}",
            songAddedEvent.RecordingId,
            songAddedEvent.PlaylistId,
            songAddedEvent.Position,
            songAddedEvent.ActorUserId.Value
        );
    }
}
