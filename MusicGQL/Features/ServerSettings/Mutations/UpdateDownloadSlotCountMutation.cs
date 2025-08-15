using MusicGQL.Features.ServerSettings.Commands;
using MusicGQL.Features.ServerSettings.Db;
using MusicGQL.Features.ServerSettings.Events;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UpdateDownloadSlotCountMutation
{
    public async Task<UpdateDownloadSlotCountResult> UpdateDownloadSlotCount(
        UpdateDownloadSlotCountInput input,
        [Service] UpdateDownloadSlotCountHandler updateDownloadSlotCountHandler,
        [Service] ServerSettingsAccessor serverSettingsAccessor,
        CancellationToken cancellationToken)
    {
        // TODO: Get actual user ID from authentication context
        var userId = Guid.Empty; // Placeholder

        var handlerResult = await updateDownloadSlotCountHandler.Handle(
            new UpdateDownloadSlotCountHandler.Command(userId, input.NewSlotCount)
        );

        return handlerResult switch
        {
            UpdateDownloadSlotCountHandler.Result.Success => await HandleSuccessAsync(),
            UpdateDownloadSlotCountHandler.Result.InvalidSlotCount =>
                new UpdateDownloadSlotCountError("Slot count must be between 1 and 10"),
            _ => throw new InvalidOperationException(
                "Unhandled result from UpdateDownloadSlotCountHandler"
            )
        };
    }

    private async Task<UpdateDownloadSlotCountResult> HandleSuccessAsync()
    {
        // For now, return a success without the server settings
        // TODO: Implement proper server settings retrieval
        return new UpdateDownloadSlotCountSuccess(null!);
    }
}

public record UpdateDownloadSlotCountInput(int NewSlotCount);

[UnionType("UpdateDownloadSlotCountResult")]
public abstract record UpdateDownloadSlotCountResult;

public record UpdateDownloadSlotCountSuccess(ServerSettings ServerSettings)
    : UpdateDownloadSlotCountResult;

public record UpdateDownloadSlotCountError(string Message)
    : UpdateDownloadSlotCountResult;
