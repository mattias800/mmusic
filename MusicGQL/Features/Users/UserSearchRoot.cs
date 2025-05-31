using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.Users;

public record UserSearchRoot
{
    [UsePaging]
    public async Task<IEnumerable<User>> GetUsers([Service] EventDbContext dbContext)
    {
        var projections = await dbContext.Users.ToListAsync();
        return projections.Select(p => new User(p));
    }
}
