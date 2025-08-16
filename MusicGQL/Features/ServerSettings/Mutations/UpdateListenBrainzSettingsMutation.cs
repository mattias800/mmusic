using HotChocolate;
using MusicGQL.Features.ServerSettings.Db;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Types;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.ServerSettings.Mutations;

public record UpdateListenBrainzSettingsInput(
    string Username,
    string ApiKey
);

public record UpdateListenBrainzSettingsResult;

public record UpdateListenBrainzSettingsSuccess : UpdateListenBrainzSettingsResult;

public record UpdateListenBrainzSettingsError(string Message) : UpdateListenBrainzSettingsResult;

[ExtendObjectType(typeof(Mutation))]
public record UpdateListenBrainzSettingsMutation
{
    public async Task<UpdateListenBrainzSettingsResult> UpdateListenBrainzSettings(
        UpdateListenBrainzSettingsInput input,
        [Service] EventDbContext dbContext
    )
    {
        try
        {
            var settings = await dbContext.ServerSettings
                .FirstOrDefaultAsync(s => s.Id == DefaultDbServerSettingsProvider.ServerSettingsSingletonId);

            if (settings == null)
            {
                // Create default settings if they don't exist
                settings = DefaultDbServerSettingsProvider.GetDefault();
                dbContext.ServerSettings.Add(settings);
            }

            settings.ListenBrainzUsername = input.Username;
            settings.ListenBrainzApiKey = input.ApiKey;

            await dbContext.SaveChangesAsync();

            return new UpdateListenBrainzSettingsSuccess();
        }
        catch (Exception ex)
        {
            return new UpdateListenBrainzSettingsError($"Failed to update ListenBrainz settings: {ex.Message}");
        }
    }
}
