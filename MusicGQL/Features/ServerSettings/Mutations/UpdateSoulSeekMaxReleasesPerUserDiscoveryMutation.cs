using MusicGQL.Features.ServerSettings.Commands;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Features.ServerSettings.Events;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UpdateSoulSeekMaxReleasesPerUserDiscoveryMutation
{
    public async Task<UpdateSoulSeekMaxReleasesPerUserDiscoveryResult> UpdateSoulSeekMaxReleasesPerUserDiscovery(
        UpdateSoulSeekMaxReleasesPerUserDiscoveryInput input,
        [Service] UpdateSoulSeekMaxReleasesPerUserDiscoveryHandler handler,
        [Service] ServerSettingsAccessor serverSettingsAccessor,
        CancellationToken cancellationToken)
    {
        // TODO: Get actual user ID from authentication context
        var userId = Guid.Empty; // Placeholder

        var handlerResult = await handler.Handle(
            new UpdateSoulSeekMaxReleasesPerUserDiscoveryHandler.Command(userId, input.MaxReleases)
        );

        return handlerResult switch
        {
            UpdateSoulSeekMaxReleasesPerUserDiscoveryHandler.Result.Success => await HandleSuccessAsync(serverSettingsAccessor),
            UpdateSoulSeekMaxReleasesPerUserDiscoveryHandler.Result.InvalidMaxReleases =>
                new UpdateSoulSeekMaxReleasesPerUserDiscoveryError("Max releases must be between 1 and 50"),
            _ => throw new InvalidOperationException(
                "Unhandled result from UpdateSoulSeekMaxReleasesPerUserDiscoveryHandler"
            )
        };
    }

    private async Task<UpdateSoulSeekMaxReleasesPerUserDiscoveryResult> HandleSuccessAsync(ServerSettingsAccessor serverSettingsAccessor)
    {
        var dbSettings = await serverSettingsAccessor.GetAsync();
        var settings = new ServerSettings(dbSettings);
        return new UpdateSoulSeekMaxReleasesPerUserDiscoverySuccess(settings);
    }
}

public record UpdateSoulSeekMaxReleasesPerUserDiscoveryInput(int MaxReleases);

[UnionType("UpdateSoulSeekMaxReleasesPerUserDiscoveryResult")]
public abstract record UpdateSoulSeekMaxReleasesPerUserDiscoveryResult;

public record UpdateSoulSeekMaxReleasesPerUserDiscoverySuccess(ServerSettings ServerSettings)
    : UpdateSoulSeekMaxReleasesPerUserDiscoveryResult;

public record UpdateSoulSeekMaxReleasesPerUserDiscoveryError(string Message)
    : UpdateSoulSeekMaxReleasesPerUserDiscoveryResult;
