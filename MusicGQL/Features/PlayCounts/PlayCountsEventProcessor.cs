using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Features.PlayCounts.Db;
using MusicGQL.Features.PlayCounts.Events;

namespace MusicGQL.Features.PlayCounts;

public class PlayCountsEventProcessor(ILogger<PlayCountsEventProcessor> logger)
{
    public async Task ProcessEvent(Event ev, EventDbContext dbContext)
    {
        if (ev is TrackPlayed trackPlayed)
        {
            await HandleTrackPlayed(trackPlayed, dbContext);
        }
    }

    private async Task HandleTrackPlayed(TrackPlayed ev, EventDbContext dbContext)
    {
        var row = await dbContext.Set<DbTrackPlayCount>()
            .FirstOrDefaultAsync(x =>
                x.ArtistId == ev.ArtistId
                && x.ReleaseFolderName == ev.ReleaseFolderName
                && x.TrackNumber == ev.TrackNumber
            );

        if (row == null)
        {
            row = new DbTrackPlayCount
            {
                ArtistId = ev.ArtistId,
                ReleaseFolderName = ev.ReleaseFolderName,
                TrackNumber = ev.TrackNumber,
                ArtistName = ev.ArtistName,
                ReleaseTitle = ev.ReleaseTitle,
                TrackTitle = ev.TrackTitle,
                PlayCount = 1,
                LastPlayedAt = ev.CreatedAt,
            };
            dbContext.Add(row);
        }
        else
        {
            row.PlayCount += 1;
            row.LastPlayedAt = ev.CreatedAt;
            // Update denormalized names if provided
            row.ArtistName = ev.ArtistName ?? row.ArtistName;
            row.ReleaseTitle = ev.ReleaseTitle ?? row.ReleaseTitle;
            row.TrackTitle = ev.TrackTitle ?? row.TrackTitle;
        }

        await dbContext.SaveChangesAsync();

        // Update per-user play counts
        var userRow = await dbContext.Set<DbUserTrackPlayCount>()
            .FirstOrDefaultAsync(x =>
                x.UserId == ev.SubjectUserId
                && x.ArtistId == ev.ArtistId
                && x.ReleaseFolderName == ev.ReleaseFolderName
                && x.TrackNumber == ev.TrackNumber
            );
        if (userRow == null)
        {
            userRow = new DbUserTrackPlayCount
            {
                UserId = ev.SubjectUserId,
                ArtistId = ev.ArtistId,
                ReleaseFolderName = ev.ReleaseFolderName,
                TrackNumber = ev.TrackNumber,
                PlayCount = 1,
                LastPlayedAt = ev.CreatedAt,
            };
            dbContext.Add(userRow);
        }
        else
        {
            userRow.PlayCount += 1;
            userRow.LastPlayedAt = ev.CreatedAt;
        }

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Incremented playcount for {ArtistId}/{ReleaseFolderName}/{TrackNumber}",
            ev.ArtistId,
            ev.ReleaseFolderName,
            ev.TrackNumber
        );
    }
}


