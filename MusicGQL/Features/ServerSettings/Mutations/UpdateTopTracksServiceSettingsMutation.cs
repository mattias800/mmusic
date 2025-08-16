using HotChocolate;
using MusicGQL.Features.ServerSettings.Db;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Types;
using MusicGQL.Db.Postgres;

namespace MusicGQL.Features.ServerSettings.Mutations;

public record UpdateTopTracksServiceSettingsInput(
    bool ListenBrainzTopTracksEnabled,
    bool SpotifyTopTracksEnabled,
    bool LastFmTopTracksEnabled
);

[UnionType]
public abstract record UpdateTopTracksServiceSettingsResult;

public record UpdateTopTracksServiceSettingsSuccess : UpdateTopTracksServiceSettingsResult
{
    public bool Success { get; } = true;
}

public record UpdateTopTracksServiceSettingsError(string Message) : UpdateTopTracksServiceSettingsResult;

[ExtendObjectType(typeof(Mutation))]
public record UpdateTopTracksServiceSettingsMutation
{
    public async Task<UpdateTopTracksServiceSettingsResult> UpdateTopTracksServiceSettings(
        UpdateTopTracksServiceSettingsInput input,
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

            settings.ListenBrainzTopTracksEnabled = input.ListenBrainzTopTracksEnabled;
            settings.SpotifyTopTracksEnabled = input.SpotifyTopTracksEnabled;
            settings.LastFmTopTracksEnabled = input.LastFmTopTracksEnabled;

            await dbContext.SaveChangesAsync();

            return new UpdateTopTracksServiceSettingsSuccess();
        }
        catch (Exception ex)
        {
            return new UpdateTopTracksServiceSettingsError($"Failed to update top tracks service settings: {ex.Message}");
        }
    }
}
