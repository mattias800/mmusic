using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Writer;
using MusicGQL.Integration.MusicBrainz;
using System.Text.Json;
using System.Text.Json.Serialization;
using Path = System.IO.Path;
using MusicGQL.Features.ServerLibrary.Cache;
using HotChocolate.Subscriptions;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class SetReleaseMatchOverrideMutation
{
    public async Task<SetReleaseMatchOverrideResult> SetReleaseMatchOverride(
        [Service] ServerLibraryJsonWriter writer,
        [Service] MusicBrainzService mbService,
        [Service] ReleaseJsonBuilder builder,
        [Service] ServerLibraryCache cache,
        [Service] ITopicEventSender eventSender,
        SetReleaseMatchOverrideInput input
    )
    {
        try
        {
            // Load current release.json (best effort)
            var releaseDir = Path.Combine("./Library", input.ArtistId, input.ReleaseFolderName);
            var jsonPath = Path.Combine(releaseDir, "release.json");

            JsonRelease? existing = null;
            if (File.Exists(jsonPath))
            {
                try
                {
                    var text = await File.ReadAllTextAsync(jsonPath);
                    existing = JsonSerializer.Deserialize<JsonRelease>(
                        text,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            PropertyNameCaseInsensitive = true,
                            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                        }
                    );
                }
                catch { }
            }

            // Ensure connections struct exists
            existing ??= new JsonRelease { Title = input.ReleaseFolderName, Type = JsonReleaseType.Album };
            existing.Connections ??= new ReleaseServiceConnections();

            // If no RG id present and we have a specific release id, fetch RG id from MB
            if (string.IsNullOrWhiteSpace(existing.Connections.MusicBrainzReleaseGroupId))
            {
                if (!string.IsNullOrWhiteSpace(input.MusicBrainzReleaseId))
                {
                    var rel = await mbService.GetReleaseByIdAsync(input.MusicBrainzReleaseId!);
                    var rgId = rel?.ReleaseGroup?.Id;
                    if (!string.IsNullOrWhiteSpace(rgId))
                    {
                        existing.Connections.MusicBrainzReleaseGroupId = rgId;
                    }
                }
            }

            // Set or clear override
            existing.Connections.MusicBrainzReleaseIdOverride = input.MusicBrainzReleaseId;

            // Persist the override immediately to release.json
            await writer.WriteReleaseAsync(input.ArtistId, input.ReleaseFolderName, existing);

            // Rebuild using builder (it will honor the override if set)
            var built = await builder.BuildAsync(
                Path.Combine("./Library", input.ArtistId),
                existing.Connections.MusicBrainzReleaseGroupId ?? (await EnsureRgIdFromOverride(mbService, input.MusicBrainzReleaseId)) ?? string.Empty,
                input.ReleaseFolderName,
                existing.Title,
                existing.Type.ToString()
            );

            if (built == null)
            {
                return new SetReleaseMatchOverrideError("Could not rebuild release after setting override");
            }

            // Ensure we keep the override value and RG id on the rebuilt JSON
            built.Connections ??= new ReleaseServiceConnections();
            built.Connections.MusicBrainzReleaseIdOverride = input.MusicBrainzReleaseId;
            built.Connections.MusicBrainzReleaseGroupId = existing.Connections.MusicBrainzReleaseGroupId
                ?? await EnsureRgIdFromOverride(mbService, input.MusicBrainzReleaseId);

            await writer.WriteReleaseAsync(input.ArtistId, input.ReleaseFolderName, built);

            // Update in-memory cache and publish metadata-updated event so clients refresh
            await cache.UpdateReleaseFromJsonAsync(input.ArtistId, input.ReleaseFolderName);
            var updated = await cache.GetReleaseByArtistAndFolderAsync(input.ArtistId, input.ReleaseFolderName);
            if (updated != null)
            {
                await eventSender.SendAsync(
                    Features.ServerLibrary.Subscription.LibrarySubscription.LibraryReleaseMetadataUpdatedTopic(input.ArtistId, input.ReleaseFolderName),
                    new Features.ServerLibrary.Release(updated)
                );
            }

            return new SetReleaseMatchOverrideSuccess(true);
        }
        catch (Exception ex)
        {
            return new SetReleaseMatchOverrideError(ex.Message);
        }
    }

    private static async Task<string?> EnsureRgIdFromOverride(MusicBrainzService mbService, string? releaseId)
    {
        if (string.IsNullOrWhiteSpace(releaseId)) return null;
        try
        {
            var rel = await mbService.GetReleaseByIdAsync(releaseId);
            return rel?.ReleaseGroup?.Id;
        }
        catch { return null; }
    }
}

public record SetReleaseMatchOverrideInput(string ArtistId, string ReleaseFolderName, string? MusicBrainzReleaseId);

[UnionType("SetReleaseMatchOverrideResult")]
public abstract record SetReleaseMatchOverrideResult;

public record SetReleaseMatchOverrideSuccess(bool Ok) : SetReleaseMatchOverrideResult;

public record SetReleaseMatchOverrideError(string Message) : SetReleaseMatchOverrideResult;
