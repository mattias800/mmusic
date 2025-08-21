using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Commands;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UpdatePublicBaseUrlMutation
{
    public async Task<UpdatePublicBaseUrlResult> UpdatePublicBaseUrl(
        UpdatePublicBaseUrlInput input,
        [Service] UpdatePublicBaseUrlHandler handler,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] EventDbContext dbContext
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return new UpdatePublicBaseUrlError("Not authenticated");
        }

        var viewer = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (viewer is null || (viewer.Roles & Users.Roles.UserRoles.Admin) == 0)
        {
            return new UpdatePublicBaseUrlError("Not authorized");
        }

        await handler.Handle(new(userId, input.PublicBaseUrl));

        var settings = await dbContext.ServerSettings.FirstOrDefaultAsync(s =>
            s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId
        ) ?? DefaultDbServerSettingsProvider.GetDefault();

        return new UpdatePublicBaseUrlSuccess(new(settings));
    }
}

public record UpdatePublicBaseUrlInput(string PublicBaseUrl);

[UnionType("UpdatePublicBaseUrlResult")]
public abstract record UpdatePublicBaseUrlResult;
public record UpdatePublicBaseUrlSuccess(ServerSettings ServerSettings) : UpdatePublicBaseUrlResult;
public record UpdatePublicBaseUrlError(string Message) : UpdatePublicBaseUrlResult;


