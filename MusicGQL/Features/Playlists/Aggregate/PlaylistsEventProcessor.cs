using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.Playlists.Db;
using MusicGQL.Features.Playlists.Events;

namespace MusicGQL.Features.Playlists.Aggregate;

public class PlaylistsEventProcessor(ILogger<PlaylistsEventProcessor> logger)
{
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

        var exists = await dbContext.Playlists.AnyAsync(p =>
            p.Id == createdPlaylist.PlaylistId && p.UserId == userId
        );

        if (exists)
        {
            logger.LogWarning(
                "Playlist {PlaylistId} already exists for user {UserId}",
                createdPlaylist.PlaylistId,
                userId
            );
            return;
        }

        var playlist = new DbPlaylist
        {
            Id = createdPlaylist.PlaylistId,
            Name = createdPlaylist.Name,
            Description = createdPlaylist.Description,
            CoverImageUrl = createdPlaylist.CoverImageUrl,
            CreatedAt = createdPlaylist.CreatedAt,
            ModifiedAt = null,
            UserId = userId,
        };

        dbContext.Playlists.Add(playlist);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Created playlist {PlaylistId} for user {UserId}",
            createdPlaylist.PlaylistId,
            userId
        );
    }

    private async Task HandleEvent(RenamedPlaylist renamePlaylistEvent, EventDbContext dbContext)
    {
        if (!renamePlaylistEvent.ActorUserId.HasValue)
            return;

        var userId = renamePlaylistEvent.ActorUserId.Value;

        var playlist = await dbContext.Playlists.FirstOrDefaultAsync(p =>
            p.Id == renamePlaylistEvent.PlaylistId && p.UserId == userId
        );

        if (playlist is null)
        {
            logger.LogWarning(
                "Playlist {PlaylistId} not found for User {UserId}",
                renamePlaylistEvent.PlaylistId,
                userId
            );
            return;
        }

        playlist.Name = renamePlaylistEvent.NewPlaylistName;
        playlist.ModifiedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Renamed Playlist {PlaylistId} to '{NewPlaylistName}' for UserId: {UserId}",
            renamePlaylistEvent.PlaylistId,
            renamePlaylistEvent.NewPlaylistName,
            userId
        );
    }

    private async Task HandleEvent(SongAddedToPlaylist songAddedEvent, EventDbContext dbContext)
    {
        if (!songAddedEvent.ActorUserId.HasValue)
            return;

        var userId = songAddedEvent.ActorUserId.Value;

        var playlist = await dbContext
            .Playlists.Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == songAddedEvent.PlaylistId && p.UserId == userId);

        if (playlist is null)
        {
            logger.LogWarning(
                "Playlist {PlaylistId} not found for User {UserId}",
                songAddedEvent.PlaylistId,
                userId
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
            userId
        );
    }
}
