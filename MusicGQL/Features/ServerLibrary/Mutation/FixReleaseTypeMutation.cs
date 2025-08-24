using System.Text.Json;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary.Mutation;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class FixReleaseTypeMutation
{
    public async Task<FixReleaseTypeResult> FixReleaseType(
        FixReleaseTypeInput input,
        [Service] ServerLibraryCache cache,
        [Service] MusicBrainzService musicBrainzService,
        [Service] ServerSettingsAccessor serverSettingsAccessor,
        [Service] ILogger<FixReleaseTypeMutation> logger
    )
    {
        try
        {
            logger.LogInformation(
                "[FixReleaseType] 🔧 Starting to fix release type for {ArtistId}/{ReleaseFolder}",
                input.ArtistId,
                input.ReleaseFolderName
            );

            // Get the release from cache
            var release = await cache.GetReleaseByArtistAndFolderAsync(
                input.ArtistId,
                input.ReleaseFolderName
            );
            if (release == null)
            {
                logger.LogWarning(
                    "[FixReleaseType] ❌ Release not found: {ArtistId}/{ReleaseFolder}",
                    input.ArtistId,
                    input.ReleaseFolderName
                );
                return new FixReleaseTypeError("Release not found");
            }

            var releaseGroupId = release.JsonRelease.Connections?.MusicBrainzReleaseGroupId;
            if (string.IsNullOrWhiteSpace(releaseGroupId))
            {
                logger.LogWarning(
                    "[FixReleaseType] ❌ Release has no MusicBrainz release group ID: {ArtistId}/{ReleaseFolder}",
                    input.ArtistId,
                    input.ReleaseFolderName
                );
                return new FixReleaseTypeError("Release has no MusicBrainz release group ID");
            }

            // Fetch the release group from MusicBrainz
            logger.LogInformation(
                "[FixReleaseType] 🔍 Fetching release group {ReleaseGroupId} from MusicBrainz",
                releaseGroupId
            );
            var releaseGroup = await musicBrainzService.GetReleaseGroupByIdAsync(releaseGroupId);
            if (releaseGroup == null)
            {
                logger.LogWarning(
                    "[FixReleaseType] ❌ Could not fetch release group from MusicBrainz: {ReleaseGroupId}",
                    releaseGroupId
                );
                return new FixReleaseTypeError("Could not fetch release group from MusicBrainz");
            }

            var musicBrainzType = releaseGroup.PrimaryType;
            logger.LogInformation(
                "[FixReleaseType] 🏷️ MusicBrainz primary type: '{MusicBrainzType}'",
                musicBrainzType
            );

            // Map the MusicBrainz type to our local type
            var newType = musicBrainzType?.ToLowerInvariant() switch
            {
                "album" => JsonReleaseType.Album,
                "ep" => JsonReleaseType.Ep,
                "single" => JsonReleaseType.Single,
                "compilation" => JsonReleaseType.Album,
                "soundtrack" => JsonReleaseType.Album,
                "live" => JsonReleaseType.Album,
                "remix" => JsonReleaseType.Ep,
                "mixtape" => JsonReleaseType.Ep,
                _ => JsonReleaseType.Album,
            };

            logger.LogInformation(
                "[FixReleaseType] 🔄 Mapping type: '{MusicBrainzType}' → {NewType}",
                musicBrainzType,
                newType
            );

            // Check if the type actually needs to change
            if (release.JsonRelease.Type == newType)
            {
                logger.LogInformation(
                    "[FixReleaseType] ℹ️ Release type is already correct: {CurrentType}",
                    release.JsonRelease.Type
                );
                return new FixReleaseTypeSuccess(release.JsonRelease);
            }

            // Update the release type
            var oldType = release.JsonRelease.Type;
            release.JsonRelease.Type = newType;

            // Get the library path and write the updated release.json
            var serverSettings = await serverSettingsAccessor.GetAsync();
            var artistPath = System.IO.Path.Combine(serverSettings.LibraryPath, input.ArtistId);
            var releasePath = System.IO.Path.Combine(artistPath, input.ReleaseFolderName);
            var releaseJsonPath = System.IO.Path.Combine(releasePath, "release.json");

            if (!File.Exists(releaseJsonPath))
            {
                logger.LogError(
                    "[FixReleaseType] ❌ Release.json file not found: {Path}",
                    releaseJsonPath
                );
                return new FixReleaseTypeError("Release.json file not found");
            }

            // Write the updated release.json
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            var updatedJson = JsonSerializer.Serialize(release.JsonRelease, jsonOptions);
            await File.WriteAllTextAsync(releaseJsonPath, updatedJson);

            logger.LogInformation(
                "[FixReleaseType] ✅ Successfully updated release type from {OldType} to {NewType}",
                oldType,
                newType
            );

            // Update the cache
            await cache.UpdateReleaseFromJsonAsync(input.ArtistId, input.ReleaseFolderName);

            return new FixReleaseTypeSuccess(release.JsonRelease);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[FixReleaseType] ❌ Error fixing release type for {ArtistId}/{ReleaseFolder}",
                input.ArtistId,
                input.ReleaseFolderName
            );
            return new FixReleaseTypeError($"Error: {ex.Message}");
        }
    }
}

public record FixReleaseTypeInput(string ArtistId, string ReleaseFolderName);

[UnionType]
public abstract record FixReleaseTypeResult;

public record FixReleaseTypeSuccess(JsonRelease Release) : FixReleaseTypeResult;

public record FixReleaseTypeError(string Message) : FixReleaseTypeResult;
