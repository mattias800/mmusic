using HotChocolate;
using System.Security.Claims;
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
    public bool Success { get; init; } = true;
    public string Message { get; init; } = "Top tracks service settings updated successfully";
}

public record UpdateTopTracksServiceSettingsError(string Message) : UpdateTopTracksServiceSettingsResult;

[ExtendObjectType(typeof(Mutation))]
public record UpdateTopTracksServiceSettingsMutation
{
    public async Task<UpdateTopTracksServiceSettingsResult> UpdateTopTracksServiceSettings(
        UpdateTopTracksServiceSettingsInput input,
        [Service] EventDbContext dbContext,
        ClaimsPrincipal claims)
    {
        try
        {
            var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null) return new UpdateTopTracksServiceSettingsError("Not authenticated");
            var userId = Guid.Parse(userIdClaim.Value);
            var viewer = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (viewer is null || (viewer.Roles & Features.Users.Roles.UserRoles.Admin) == 0)
            {
                return new UpdateTopTracksServiceSettingsError("Not authorized");
            }

            var settings = await dbContext.ServerSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return new UpdateTopTracksServiceSettingsError("Server settings not found");
            }

            // Update the settings
            settings.ListenBrainzTopTracksEnabled = input.ListenBrainzTopTracksEnabled;
            settings.SpotifyTopTracksEnabled = input.SpotifyTopTracksEnabled;
            settings.LastFmTopTracksEnabled = input.LastFmTopTracksEnabled;

            await dbContext.SaveChangesAsync();

            var enabledServices = new List<string>();
            if (input.ListenBrainzTopTracksEnabled) enabledServices.Add("ListenBrainz");
            if (input.SpotifyTopTracksEnabled) enabledServices.Add("Spotify");
            if (input.LastFmTopTracksEnabled) enabledServices.Add("Last.fm");

            var message = enabledServices.Count > 0 
                ? $"Top tracks services updated. Enabled services: {string.Join(", ", enabledServices)}"
                : "Top tracks services updated. All services are now disabled.";

            return new UpdateTopTracksServiceSettingsSuccess { Message = message };
        }
        catch (Exception ex)
        {
            return new UpdateTopTracksServiceSettingsError($"Failed to update top tracks service settings: {ex.Message}");
        }
    }
}
