using System.Text.Json;
using System.Text.Json.Serialization;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Service responsible for importing a single release group and writing release.json
/// Also enriches tracks with per-track Last.fm play count when available.
/// </summary>
public class LibraryReleaseImportService(
    ReleaseJsonBuilder releaseJsonBuilder,
    ServerLibrary.Writer.ServerLibraryJsonWriter writer,
    ILogger<LibraryReleaseImportService> logger
)
{
    private static JsonSerializerOptions GetJsonOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

    public async Task<SingleReleaseImportResult> ImportReleaseGroupAsync(
        MusicBrainzReleaseGroupResult releaseGroup,
        string artistFolderPath,
        string artistId
    )
    {
        var startTime = DateTime.UtcNow;
        var result = new SingleReleaseImportResult
        {
            ReleaseGroupId = releaseGroup.Id,
            Title = releaseGroup.Title,
        };

        logger.LogInformation(
            "[ReleaseImport] 🚀 Starting import of release group '{Title}' (Type: {PrimaryType}, ID: {ReleaseGroupId}) for artist '{ArtistId}'",
            releaseGroup.Title,
            releaseGroup.PrimaryType,
            releaseGroup.Id,
            artistId
        );

        // Declare releaseFolderPath at method level so it's accessible in catch block
        string releaseFolderPath = string.Empty;

        try
        {
            // 1. Create release folder
            logger.LogInformation("[ReleaseImport] 📁 Step 1: Creating release folder structure");
            var releaseFolderName = SanitizeFolderName(releaseGroup.Title);
            releaseFolderPath = Path.Combine(artistFolderPath, releaseFolderName);

            if (!Directory.Exists(releaseFolderPath))
            {
                Directory.CreateDirectory(releaseFolderPath);
                logger.LogInformation(
                    "[ReleaseImport] ✅ Created new release directory: {ReleasePath}",
                    releaseFolderPath
                );
            }
            else
            {
                logger.LogInformation(
                    "[ReleaseImport] ℹ️ Release directory already exists: {ReleasePath}",
                    releaseFolderPath
                );
            }

            // 2. Build release.json using centralized builder
            logger.LogInformation(
                "[ReleaseImport] 🔨 Step 2: Building release.json metadata for '{Title}'",
                releaseGroup.Title
            );
            var buildStart = DateTime.UtcNow;

            var built = await releaseJsonBuilder.BuildAsync(
                artistFolderPath,
                releaseGroup.Id,
                releaseFolderName,
                releaseGroup.Title,
                releaseGroup.PrimaryType
            );

            var buildDuration = DateTime.UtcNow - buildStart;

            if (built is null)
            {
                logger.LogWarning(
                    "[ReleaseImport] ⚠️ No suitable release found for release group '{Title}' after {DurationMs}ms",
                    releaseGroup.Title,
                    buildDuration.TotalMilliseconds
                );
                result.ErrorMessage = "No suitable release found for release group";
                return result;
            }

            logger.LogInformation(
                "[ReleaseImport] ✅ Successfully built release.json in {DurationMs}ms",
                buildDuration.TotalMilliseconds
            );

            // 3. Write via centralized writer
            logger.LogInformation("[ReleaseImport] 💾 Step 3: Writing release.json to disk");
            var writeStart = DateTime.UtcNow;

            await writer.WriteReleaseAsync(artistId, releaseFolderName, built);

            var writeDuration = DateTime.UtcNow - writeStart;
            logger.LogInformation(
                "[ReleaseImport] ✅ Successfully wrote release.json in {DurationMs}ms",
                writeDuration.TotalMilliseconds
            );

            result.Success = true;
            result.ReleaseFolderPath = releaseFolderPath;
            result.ReleaseJson = built;

            var totalDuration = DateTime.UtcNow - startTime;
            logger.LogInformation(
                "[ReleaseImport] 🎉 Successfully imported release '{Title}' in {TotalDurationMs}ms",
                releaseGroup.Title,
                totalDuration.TotalMilliseconds
            );
            logger.LogInformation(
                "[ReleaseImport] 📊 Import Summary: Build: {BuildMs}ms, Write: {WriteMs}ms",
                buildDuration.TotalMilliseconds,
                writeDuration.TotalMilliseconds
            );
        }
        catch (Exception ex)
        {
            var totalDuration = DateTime.UtcNow - startTime;
            logger.LogError(
                ex,
                "[ReleaseImport] ❌ Failed to import release '{Title}' after {TotalDurationMs}ms",
                releaseGroup.Title,
                totalDuration.TotalMilliseconds
            );

            result.ErrorMessage = ex.Message;

            // Cleanup on error
            if (!string.IsNullOrEmpty(releaseFolderPath))
            {
                logger.LogInformation(
                    "[ReleaseImport] 🧹 Cleaning up failed release directory: {ReleasePath}",
                    releaseFolderPath
                );
                if (Directory.Exists(releaseFolderPath))
                {
                    try
                    {
                        Directory.Delete(releaseFolderPath, true);
                        logger.LogInformation(
                            "[ReleaseImport] ✅ Successfully cleaned up failed release directory"
                        );
                    }
                    catch (Exception cleanupEx)
                    {
                        logger.LogWarning(
                            cleanupEx,
                            "[ReleaseImport] ⚠️ Failed to clean up release directory: {ReleasePath}",
                            releaseFolderPath
                        );
                    }
                }
            }
        }

        return result;
    }

    // Rebuilds a specific release.json in-place for an existing folder using a known release group id
    public async Task<SingleReleaseImportResult> ImportReleaseGroupInPlaceAsync(
        string releaseGroupId,
        string releaseTitle,
        string? primaryType,
        string artistFolderPath,
        string artistId,
        string releaseFolderName
    )
    {
        var result = new SingleReleaseImportResult
        {
            ReleaseGroupId = releaseGroupId,
            Title = releaseTitle,
        };

        var releaseFolderPath = Path.Combine(artistFolderPath, releaseFolderName);
        if (!Directory.Exists(releaseFolderPath))
        {
            Directory.CreateDirectory(releaseFolderPath);
        }

        try
        {
            var built = await releaseJsonBuilder.BuildAsync(
                artistFolderPath,
                releaseGroupId,
                releaseFolderName,
                releaseTitle,
                primaryType
            );

            if (built is null)
            {
                result.ErrorMessage = "No suitable release found for release group";
                return result;
            }

            await writer.WriteReleaseAsync(artistId, releaseFolderName, built);

            result.Success = true;
            result.ReleaseFolderPath = releaseFolderPath;
            result.ReleaseJson = built;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private static string SanitizeFolderName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join(
            "",
            name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)
        );
        return sanitized.Trim();
    }
}
