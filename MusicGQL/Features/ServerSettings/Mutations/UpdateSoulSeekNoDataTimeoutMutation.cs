using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Events;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

public record UpdateSoulSeekNoDataTimeoutInput(int Seconds);

[ExtendObjectType(typeof(Mutation))]
public class UpdateSoulSeekNoDataTimeoutMutation
{
    public async Task<UpdateSoulSeekNoDataTimeoutResult> UpdateSoulSeekNoDataTimeout(
        UpdateSoulSeekNoDataTimeoutInput input,
        EventDbContext db,
        EventProcessor.EventProcessorWorker eventProcessorWorker
    )
    {
        if (input.Seconds is < 5 or > 600)
        {
            return new UpdateSoulSeekNoDataTimeoutError("Seconds out of range (5-600)");
        }

        db.Events.Add(new SoulSeekNoDataTimeoutUpdated { NewSeconds = input.Seconds });

        await db.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();

        // Build ServerSettings GQL object from latest row
        var serverSettings =
            await db.ServerSettings.FirstOrDefaultAsync(s =>
                s.Id == Db.DefaultDbServerSettingsProvider.ServerSettingsSingletonId
            ) ?? Db.DefaultDbServerSettingsProvider.GetDefault();

        // Mirror in returned instance
        serverSettings.SoulSeekNoDataTimeoutSeconds = input.Seconds;

        return new UpdateSoulSeekNoDataTimeoutSuccess(new ServerSettings(serverSettings));
    }
}

[UnionType("UpdateSoulSeekNoDataTimeoutResult")]
public abstract record UpdateSoulSeekNoDataTimeoutResult;

public record UpdateSoulSeekNoDataTimeoutSuccess(ServerSettings ServerSettings)
    : UpdateSoulSeekNoDataTimeoutResult;

public record UpdateSoulSeekNoDataTimeoutError(string Message) : UpdateSoulSeekNoDataTimeoutResult;
