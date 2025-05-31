using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.Playlists.Db;
using MusicGQL.Features.Playlists.Events;

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
            return;

        var userId = createdPlaylist.ActorUserId.Value;

        var playlistsForUser = await dbContext.PlaylistsForUser.FindAsync(userId);

        if (playlistsForUser is null)
        {
            playlistsForUser = new PlaylistsForUser { UserId = userId };

            dbContext.PlaylistsForUser.Add(playlistsForUser);
        }

        if (playlistsForUser.Playlists.Any(p => p.Id == createdPlaylist.PlaylistId))
        {
            logger.LogWarning(
                "Playlist {PlaylistId} already exists for user {UserId}",
                createdPlaylist.PlaylistId,
                userId
            );
            return;
        }

        playlistsForUser.Playlists.Add(
            new DbPlaylist
            {
                Id = createdPlaylist.PlaylistId,
                Name = createdPlaylist.Name,
                Description = createdPlaylist.Description,
                CoverImageUrl = createdPlaylist.CoverImageUrl,
                CreatedAt = createdPlaylist.CreatedAt,
            }
        );
    }

    private async Task HandleEvent(RenamedPlaylist renamePlaylistEvent, EventDbContext dbContext)
    {
        if (!renamePlaylistEvent.ActorUserId.HasValue)
            return;

        var projection = await dbContext
            .PlaylistsForUser.Include(p => p.Playlists)
            .FirstOrDefaultAsync(p =>
                p.UserId == renamePlaylistEvent.ActorUserId.Value
                && p.Playlists.Any(pl => pl.Id == renamePlaylistEvent.PlaylistId)
            );

        if (projection is null)
            return;

        var playlist = projection.Playlists.FirstOrDefault(pl =>
            pl.Id == renamePlaylistEvent.PlaylistId
        );
        if (playlist is null)
            return;

        playlist.Name = renamePlaylistEvent.NewPlaylistName;
        playlist.ModifiedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

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
            return;

        var userPlaylists = await dbContext
            .PlaylistsForUser.Include(p => p.Playlists)
            .ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(p => p.UserId == songAddedEvent.ActorUserId.Value);

        if (userPlaylists is null)
        {
            logger.LogWarning("User {UserId} not found", songAddedEvent.ActorUserId.Value);
            return;
        }

        var playlist = userPlaylists.Playlists.FirstOrDefault(p =>
            p.Id == songAddedEvent.PlaylistId
        );
        if (playlist is null)
        {
            logger.LogWarning(
                "Playlist {PlaylistId} not found for User {UserId}",
                songAddedEvent.PlaylistId,
                songAddedEvent.ActorUserId.Value
            );
            return;
        }

        playlist.Items.Add(
            new DbPlaylistItem
            {
                RecordingId = songAddedEvent.RecordingId,
                AddedAt = DateTime.UtcNow,
            }
        );

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Added recording {RecordingId} to Playlist {PlaylistId} at position {Position} for UserId: {UserId}",
            songAddedEvent.RecordingId,
            songAddedEvent.PlaylistId,
            songAddedEvent.Position,
            songAddedEvent.ActorUserId.Value
        );
    }
}
