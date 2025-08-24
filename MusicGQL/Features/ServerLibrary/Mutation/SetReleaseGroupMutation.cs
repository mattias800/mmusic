using System.Text.Json;
using System.Text.Json.Serialization;
using HotChocolate.Subscriptions;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Writer;
using MusicGQL.Features.ServerSettings;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class SetReleaseGroupMutation
{
    public async Task<SetReleaseGroupResult> SetReleaseGroup(
        [Service] ServerLibraryCache cache,
        [Service] ReleaseJsonBuilder builder,
        [Service] ServerLibraryJsonWriter writer,
        [Service] ITopicEventSender eventSender,
        [Service] ServerSettingsAccessor serverSettingsAccessor,
        SetReleaseGroupInput input
    )
    {
        try
        {
            var release = await cache.GetReleaseByArtistAndFolderAsync(
                input.ArtistId,
                input.ReleaseFolderName
            );
            if (release == null)
            {
                return new SetReleaseGroupError("Release not found");
            }

            // Load existing JSON to preserve audio file paths if possible
            var relJsonPath = Path.Combine(release.ReleasePath, "release.json");
            JsonRelease? existing = null;
            try
            {
                if (File.Exists(relJsonPath))
                {
                    var text = await File.ReadAllTextAsync(relJsonPath);
                    existing = JsonSerializer.Deserialize<JsonRelease>(
                        text,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            PropertyNameCaseInsensitive = true,
                            Converters =
                            {
                                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                            },
                        }
                    );
                }
            }
            catch { }

            // Ensure connections block exists and set new RG id; clear specific release override
            existing ??= new JsonRelease { Title = release.Title, Type = JsonReleaseType.Album };

            // Set artist name if not already set
            if (string.IsNullOrWhiteSpace(existing.ArtistName))
            {
                try
                {
                    var artistJsonPath = Path.Combine(release.ReleasePath, "..", "artist.json");
                    if (File.Exists(artistJsonPath))
                    {
                        var text = await File.ReadAllTextAsync(artistJsonPath);
                        var jsonArtist = JsonSerializer.Deserialize<JsonArtist>(
                            text,
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                PropertyNameCaseInsensitive = true,
                                Converters =
                                {
                                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                                },
                            }
                        );
                        existing.ArtistName = jsonArtist?.Name ?? string.Empty;
                    }
                }
                catch
                {
                    existing.ArtistName = string.Empty;
                }
            }

            existing.Connections ??= new ReleaseServiceConnections();
            existing.Connections.MusicBrainzReleaseGroupId = input.MusicBrainzReleaseGroupId;
            existing.Connections.MusicBrainzReleaseIdOverride = null;

            // Persist immediate update so JSON always has desired RG id
            await writer.WriteReleaseAsync(input.ArtistId, input.ReleaseFolderName, existing);

            // Rebuild JSON using the centralized builder (this will select a non-demo main release)
            var built = await builder.BuildAsync(
                Path.Combine((await serverSettingsAccessor.GetAsync()).LibraryPath, input.ArtistId),
                input.MusicBrainzReleaseGroupId,
                input.ReleaseFolderName,
                existing.Title,
                existing.Type.ToString()
            );

            if (built == null)
            {
                return new SetReleaseGroupError("Failed to rebuild after setting release group");
            }

            // Preserve audio file paths by track number if possible
            if (existing?.Tracks != null && existing.Tracks.Count > 0)
            {
                var byNumber = existing
                    .Tracks.Where(t => t != null)
                    .ToDictionary(t => t.TrackNumber, t => t.AudioFilePath);

                if (built.Tracks != null)
                {
                    foreach (var t in built.Tracks)
                    {
                        if (t == null)
                            continue;
                        if (
                            byNumber.TryGetValue(t.TrackNumber, out var oldPath)
                            && !string.IsNullOrWhiteSpace(oldPath)
                        )
                        {
                            t.AudioFilePath = oldPath;
                        }
                    }
                }
            }

            await writer.WriteReleaseAsync(input.ArtistId, input.ReleaseFolderName, built);

            // Update cache and notify subscribers
            await cache.UpdateReleaseFromJsonAsync(input.ArtistId, input.ReleaseFolderName);
            var updated = await cache.GetReleaseByArtistAndFolderAsync(
                input.ArtistId,
                input.ReleaseFolderName
            );
            if (updated != null)
            {
                await eventSender.SendAsync(
                    Subscription.LibrarySubscription.LibraryReleaseMetadataUpdatedTopic(
                        input.ArtistId,
                        input.ReleaseFolderName
                    ),
                    new Release(updated)
                );

                await eventSender.SendAsync(
                    Subscription.LibrarySubscription.LibraryReleaseUpdatedTopic(
                        input.ArtistId,
                        input.ReleaseFolderName
                    ),
                    new Release(updated)
                );
                await eventSender.SendAsync(
                    Subscription.LibrarySubscription.LibraryArtistReleaseUpdatedTopic(
                        input.ArtistId
                    ),
                    new Release(updated)
                );
            }

            return updated != null
                ? new SetReleaseGroupSuccess(new Release(updated))
                : new SetReleaseGroupError("Release not found after update");
        }
        catch (Exception ex)
        {
            return new SetReleaseGroupError(ex.Message);
        }
    }
}

public record SetReleaseGroupInput(
    string ArtistId,
    string ReleaseFolderName,
    string MusicBrainzReleaseGroupId
);

[UnionType("SetReleaseGroupResult")]
public abstract record SetReleaseGroupResult;

public record SetReleaseGroupSuccess(Release Release) : SetReleaseGroupResult;

public record SetReleaseGroupError(string Message) : SetReleaseGroupResult;
