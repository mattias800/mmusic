using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Commands;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UpdateLibraryPathMutation
{
    public async Task<UpdateLibraryPathResult> UpdateLibraryPath(
        UpdateLibraryPathInput input,
        [Service] UpdateLibraryPathHandler updateLibraryPathHandler,
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
            // This could also be a specific UpdateLibraryPathResult type like UpdateLibraryPathResult.NotAuthenticated
            throw new UnauthorizedAccessException("User not authenticated.");
        }

        var handlerResult = await updateLibraryPathHandler.Handle(
            new UpdateLibraryPathHandler.Command(userId, input.NewLibraryPath)
        );

        switch (handlerResult)
        {
            case UpdateLibraryPathHandler.Result.Success:
                var serverSettings =
                    await dbContext.ServerSettings.FirstOrDefaultAsync(s =>
                        s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId
                    ) ?? DefaultDbServerSettingsProvider.GetDefault();

                return new UpdateLibraryPathResult.UpdateLibraryPathSuccess(new(serverSettings));
            default:
                // Log error: Unhandled handler result
                throw new ArgumentOutOfRangeException(
                    nameof(handlerResult),
                    "Unhandled result from UpdateLibraryPathHandler"
                );
        }
    }
}

public record UpdateLibraryPathInput(string NewLibraryPath);

[UnionType("UpdateLibraryPathResult")]
public abstract record UpdateLibraryPathResult
{
    public record UpdateLibraryPathSuccess(ServerSettings ServerSettings) : UpdateLibraryPathResult;
}
