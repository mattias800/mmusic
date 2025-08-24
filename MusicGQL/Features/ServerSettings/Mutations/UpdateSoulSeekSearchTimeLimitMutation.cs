using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerSettings.Events;
using MusicGQL.Types;

namespace MusicGQL.Features.ServerSettings.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class UpdateSoulSeekSearchTimeLimitMutation
{
    public async Task<UpdateSoulSeekSearchTimeLimitResult> UpdateSoulSeekSearchTimeLimit(
        EventDbContext db,
        EventProcessor.EventProcessorWorker eventProcessorWorker,
        int seconds
    )
    {
        if (seconds is < 5 or > 600)
        {
            return new UpdateSoulSeekSearchTimeLimitError("Seconds out of range (5-600)");
        }

        db.Events.Add(new SoulSeekSearchTimeLimitUpdated { NewSeconds = seconds });

        await db.SaveChangesAsync();
        await eventProcessorWorker.ProcessEvents();

        // Build ServerSettings GQL object from latest row
        var serverSettings =
            await db.ServerSettings.FirstOrDefaultAsync(s =>
                s.Id == Db.DefaultDbServerSettingsProvider.ServerSettingsSingletonId
            ) ?? Db.DefaultDbServerSettingsProvider.GetDefault();

        // Mirror the new seconds in the instance we return
        serverSettings.SoulSeekSearchTimeLimitSeconds = seconds;

        return new UpdateSoulSeekSearchTimeLimitSuccess(new ServerSettings(serverSettings));
    }
}

[UnionType("UpdateSoulSeekSearchTimeLimitResult")]
public abstract record UpdateSoulSeekSearchTimeLimitResult;

public record UpdateSoulSeekSearchTimeLimitSuccess(ServerSettings ServerSettings)
    : UpdateSoulSeekSearchTimeLimitResult;

public record UpdateSoulSeekSearchTimeLimitError(string Message)
    : UpdateSoulSeekSearchTimeLimitResult;
