using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Commands;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UpdateDownloadPathMutation
{
    public async Task<UpdateDownloadPathResult> UpdateDownloadPath(
        UpdateDownloadPathInput input,
        [Service] UpdateDownloadPathHandler updateDownloadPathHandler,
        [Service] IHttpContextAccessor httpContextAccessor, // Inject IHttpContextAccessor
        [Service] EventDbContext dbContext // Inject EventDbContext to fetch viewer details
    )
    {
        var userIdString = httpContextAccessor.HttpContext?.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );
        if (!Guid.TryParse(userIdString, out var userId))
        {
            // User not authenticated or UserId claim is missing/invalid
            // Return an appropriate error. For now, throwing, but a GraphQL error object is better.
            // This could also be a specific UpdateDownloadPathResult type like UpdateDownloadPathResult.NotAuthenticated
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        var handlerResult = await updateDownloadPathHandler.Handle(
            new UpdateDownloadPathHandler.Command(userId, input.NewDownloadPath)
        );

        switch (handlerResult)
        {
            case UpdateDownloadPathHandler.Result.Success:
                var serverSettings =
                    await dbContext.ServerSettings.FirstOrDefaultAsync(s =>
                        s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId
                    ) ?? DefaultDbServerSettingsProvider.GetDefault();

                return new UpdateDownloadPathSuccess(new(serverSettings));
            default:
                // Log error: Unhandled handler result
                throw new ArgumentOutOfRangeException(
                    nameof(handlerResult),
                    "Unhandled result from UpdateDownloadPathHandler"
                );
        }
    }
}

public record UpdateDownloadPathInput(string NewDownloadPath);

[UnionType("UpdateDownloadPathResult")]
public abstract record UpdateDownloadPathResult;

public record UpdateDownloadPathSuccess(ServerSettings ServerSettings) : UpdateDownloadPathResult;
