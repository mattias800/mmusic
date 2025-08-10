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

            case SongRemovedFromPlaylist e:
                await HandleEvent(e, dbContext);
                break;

            case PlaylistItemMoved e:
                await HandleEvent(e, dbContext);
                break;

            case DeletedPlaylist e:
                await HandleEvent(e, dbContext);
                break;
        }
    }

    private async Task HandleEvent(CreatedPlaylist ev, EventDbContext dbContext)
    {
        if (!ev.ActorUserId.HasValue)
            return;

        var userId = ev.ActorUserId.Value;

        var exists = await dbContext.Playlists.AnyAsync(p =>
            p.Id == ev.PlaylistId && p.UserId == userId
        );

        if (exists)
        {
            logger.LogWarning(
                "Playlist {PlaylistId} already exists for user {UserId}",
                ev.PlaylistId,
                userId
            );
            return;
        }

        var playlist = new DbPlaylist
        {
            Id = ev.PlaylistId,
            Name = ev.Name,
            Description = ev.Description,
            CoverImageUrl = ev.CoverImageUrl,
            CreatedAt = ev.CreatedAt,
            ModifiedAt = null,
            UserId = userId,
        };

        dbContext.Playlists.Add(playlist);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Created playlist {PlaylistId} for user {UserId}",
            ev.PlaylistId,
            userId
        );
    }

    private async Task HandleEvent(RenamedPlaylist ev, EventDbContext dbContext)
    {
        if (!ev.ActorUserId.HasValue)
            return;

        var userId = ev.ActorUserId.Value;

        var playlist = await dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == ev.PlaylistId);

        if (playlist is null)
        {
            logger.LogWarning("Playlist {PlaylistId} not found", ev.PlaylistId);
            return;
        }

        playlist.Name = ev.NewPlaylistName;
        playlist.ModifiedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Renamed Playlist {PlaylistId} to '{NewPlaylistName}' for UserId: {UserId}",
            ev.PlaylistId,
            ev.NewPlaylistName,
            userId
        );
    }

    private async Task HandleEvent(DeletedPlaylist ev, EventDbContext dbContext)
    {
        if (!ev.ActorUserId.HasValue)
            return;

        var userId = ev.ActorUserId.Value;

        var playlist = await dbContext.Playlists.FirstOrDefaultAsync(p => p.Id == ev.PlaylistId);

        if (playlist is null)
        {
            logger.LogWarning("Playlist {PlaylistId} not found", ev.PlaylistId);
            return;
        }

        dbContext.Playlists.Remove(playlist);

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Deleted Playlist {PlaylistId} for UserId: {UserId}",
            ev.PlaylistId,
            userId
        );
    }

    private async Task HandleEvent(SongAddedToPlaylist ev, EventDbContext dbContext)
    {
        if (!ev.ActorUserId.HasValue)
            return;

        var userId = ev.ActorUserId.Value;

        var playlist = await dbContext
            .Playlists.Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == ev.PlaylistId && p.UserId == userId);

        if (playlist is null)
        {
            logger.LogWarning(
                "Playlist {PlaylistId} not found for User {UserId}",
                ev.PlaylistId,
                userId
            );
            return;
        }

        playlist.Items.Add(
            new DbPlaylistItem { RecordingId = ev.RecordingId, AddedAt = DateTime.UtcNow }
        );

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Added recording {RecordingId} to Playlist {PlaylistId} at position {Position} for UserId: {UserId}",
            ev.RecordingId,
            ev.PlaylistId,
            ev.Position,
            userId
        );
    }

    private async Task HandleEvent(SongRemovedFromPlaylist ev, EventDbContext dbContext)
    {
        if (!ev.ActorUserId.HasValue)
            return;

        var userId = ev.ActorUserId.Value;

        var playlist = await dbContext
            .Playlists.Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == ev.PlaylistId && p.UserId == userId);

        if (playlist is null)
        {
            logger.LogWarning(
                "Playlist {PlaylistId} not found for User {UserId}",
                ev.PlaylistId,
                userId
            );
            return;
        }

        var item = playlist.Items.FirstOrDefault(i => i.RecordingId == ev.RecordingId);
        if (item != null)
        {
            dbContext.Remove(item);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task HandleEvent(PlaylistItemMoved ev, EventDbContext dbContext)
    {
        if (!ev.ActorUserId.HasValue)
            return;

        var userId = ev.ActorUserId.Value;

        var playlist = await dbContext
            .Playlists.Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == ev.PlaylistId && p.UserId == userId);

        if (playlist is null)
        {
            logger.LogWarning(
                "Playlist {PlaylistId} not found for User {UserId}",
                ev.PlaylistId,
                userId
            );
            return;
        }

        // Simple reorder: remove all, then insert recording at new index maintaining order
        var item = playlist.Items.FirstOrDefault(i => i.RecordingId == ev.RecordingId);
        if (item == null)
            return;
        playlist.Items.Remove(item);
        var newIndex = Math.Clamp(ev.NewIndex, 0, playlist.Items.Count);
        playlist.Items.Insert(newIndex, item);
        await dbContext.SaveChangesAsync();
    }
}
