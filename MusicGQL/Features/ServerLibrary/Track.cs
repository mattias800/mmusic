using Microsoft.EntityFrameworkCore;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.ServerLibrary;

public record Track([property: GraphQLIgnore] CachedTrack Model)
{
    [ID]
    public string Id() => Model.ArtistId + "/" + Model.ReleaseFolderName + "/" + Model.DiscNumber + ":" + Model.TrackNumber;

    public string Title() => Model.Title;

    public int TrackNumber() => Model.JsonTrack.TrackNumber;

    public int DiscNumber() => Model.DiscNumber;

    public int? TrackLength() => Model.JsonTrack.TrackLength;

    public async Task<long?> PlayCount([Service] Db.Postgres.EventDbContext db)
    {
        var row = await db.Set<MusicGQL.Features.PlayCounts.Db.DbTrackPlayCount>()
            .FirstOrDefaultAsync(x =>
                x.ArtistId == Model.ArtistId
                && x.ReleaseFolderName == Model.ReleaseFolderName
                && x.TrackNumber == Model.TrackNumber
            );
        return row?.PlayCount ?? 0;
    }

    public async Task<long?> PlayCountForViewer(
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] Db.Postgres.EventDbContext db
    )
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity == null || !httpContext.User.Identity.IsAuthenticated)
            return 0;
        var userIdClaim = httpContext.User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier
        );
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return 0;
        var row = await db.Set<MusicGQL.Features.PlayCounts.Db.DbUserTrackPlayCount>()
            .FirstOrDefaultAsync(x =>
                x.UserId == userId
                && x.ArtistId == Model.ArtistId
                && x.ReleaseFolderName == Model.ReleaseFolderName
                && x.TrackNumber == Model.TrackNumber
            );
        return row?.PlayCount ?? 0;
    }

    public bool IsMissing() => string.IsNullOrWhiteSpace(Model.JsonTrack.AudioFilePath);

    public async Task<Release> Release(ServerLibraryCache cache)
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(
            Model.ArtistId,
            Model.ReleaseFolderName
        );

        if (release is null)
        {
            throw new Exception(
                "Could not find release, artistId="
                    + Model.ArtistId
                    + ", folderName="
                    + Model.ReleaseFolderName
            );
        }

        return new(release);
    }

    public TrackMedia? Media() =>
        string.IsNullOrWhiteSpace(Model.JsonTrack.AudioFilePath) ? null : new(Model);

    public MediaAvailabilityStatus MediaAvailabilityStatus() =>
        Model.CachedMediaAvailabilityStatus.ToGql();

    public IEnumerable<TrackCredit> Credits() =>
        Model.JsonTrack.Credits?.Select(t => new TrackCredit(t)) ?? [];

    public TrackStatistics? Statistics()
    {
        return Model.JsonTrack.Statistics is null
            ? null
            : new TrackStatistics(Model.JsonTrack.Statistics);
    }
}
