using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Writer;
using System.Text.Json;
using System.Text.Json.Serialization;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class RefreshReleaseMutation
{
    public async Task<RefreshReleaseResult> RefreshRelease(
        [Service] ServerLibraryCache cache,
        [Service] MusicBrainzImportService mbImport,
        [Service] LibraryReleaseImportService releaseImporter,
        RefreshReleaseInput input
    )
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );
        if (release == null)
        {
            return new RefreshReleaseError("Release not found");
        }

        // Load existing release.json to preserve local audio file references
        var releaseJsonPath = Path.Combine(release.ReleasePath, "release.json");
        JsonRelease? existingRelease = null;
        try
        {
            if (File.Exists(releaseJsonPath))
            {
                var text = await File.ReadAllTextAsync(releaseJsonPath);
                existingRelease = JsonSerializer.Deserialize<JsonRelease>(
                    text,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                    }
                );
            }
        }
        catch
        {
            // If parsing fails, continue without preservation (best effort)
        }

        // Find release group to import using MusicBrainz (by title under artist MBID)
        var artist = await cache.GetArtistByIdAsync(input.ArtistId);
        var mbArtistId = artist?.JsonArtist.Connections?.MusicBrainzArtistId;
        if (string.IsNullOrWhiteSpace(mbArtistId))
        {
            return new RefreshReleaseError("Artist missing MusicBrainz ID");
        }

        var rgs = await mbImport.GetArtistReleaseGroupsAsync(mbArtistId);
        var match = rgs.FirstOrDefault(rg =>
            string.Equals(rg.Title, release.Title, StringComparison.OrdinalIgnoreCase)
        );
        if (match == null)
        {
            return new RefreshReleaseError("Could not match release group by title");
        }

        var importResult = await releaseImporter.ImportReleaseGroupAsync(
            match,
            Path.GetDirectoryName(release.ReleasePath) ?? Path.Combine("./Library", input.ArtistId),
            input.ArtistId
        );

        // After importer writes new release.json, restore audio file references from the previous JSON
        if (existingRelease?.Tracks != null && existingRelease.Tracks.Count > 0)
        {
            var byNumber = existingRelease.Tracks
                .Where(t => t != null)
                .ToDictionary(t => t!.TrackNumber, t => t!.AudioFilePath);

            var writer = new ServerLibraryJsonWriter();
            await writer.UpdateReleaseAsync(
                input.ArtistId,
                input.ReleaseFolderName,
                rel =>
                {
                    if (rel.Tracks == null) return;
                    foreach (var t in rel.Tracks)
                    {
                        if (t == null) continue;
                        if (byNumber.TryGetValue(t.TrackNumber, out var oldPath) && !string.IsNullOrWhiteSpace(oldPath))
                        {
                            t.AudioFilePath = oldPath;
                        }
                    }
                }
            );
        }

        // Only update this release in cache (keep statuses)
        await cache.UpdateReleaseFromJsonAsync(input.ArtistId, input.ReleaseFolderName);

        var releaseAfterRefresh = await cache.GetReleaseByArtistAndFolderAsync(
            input.ArtistId,
            input.ReleaseFolderName
        );

        if (releaseAfterRefresh == null)
        {
            return new RefreshReleaseError("Release does not exist after refreshing metadata.");
        }

        return importResult.Success
            ? new RefreshReleaseSuccess(new(releaseAfterRefresh))
            : new RefreshReleaseError(importResult.ErrorMessage ?? "Unknown error");
    }
}

public record RefreshReleaseInput(string ArtistId, string ReleaseFolderName);

[UnionType("RefreshReleaseResult")]
public abstract record RefreshReleaseResult;

public record RefreshReleaseSuccess(Release Release) : RefreshReleaseResult;

public record RefreshReleaseError(string Message) : RefreshReleaseResult;
