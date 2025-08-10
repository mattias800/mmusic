using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.Playlists.Commands;

public class VerifyPlaylistWriteAccessHandler(EventDbContext dbContext)
{
    public async Task<Result> VerifyPlaylistWriteAccess(Query query)
    {
        var allowed = await dbContext.Playlists.AnyAsync(p =>
            p.Id == query.PlaylistId && p.UserId == query.UserId
        );
        return allowed switch
        {
            true => new Result.WritesAllowed(),
            _ => new Result.WritesNotAllowed(),
        };
    }

    public record Query(Guid UserId, string PlaylistId);

    public abstract record Result
    {
        public record WritesAllowed : Result;

        public record WritesNotAllowed : Result;
    }
}
