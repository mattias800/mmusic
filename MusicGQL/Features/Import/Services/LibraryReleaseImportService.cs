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
    ServerLibrary.Writer.ServerLibraryJsonWriter writer
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
        var result = new SingleReleaseImportResult
        {
            ReleaseGroupId = releaseGroup.Id,
            Title = releaseGroup.Title,
        };

        // 1. Create release folder
        var releaseFolderName = SanitizeFolderName(releaseGroup.Title);
        var releaseFolderPath = Path.Combine(artistFolderPath, releaseFolderName);

        if (!Directory.Exists(releaseFolderPath))
        {
            Directory.CreateDirectory(releaseFolderPath);
        }

        try
        {
            // Build release.json using centralized builder
            var built = await releaseJsonBuilder.BuildAsync(
                artistFolderPath,
                releaseGroup.Id,
                releaseFolderName,
                releaseGroup.Title,
                releaseGroup.PrimaryType
            );

            if (built is null)
            {
                result.ErrorMessage = "No suitable release found for release group";
                return result;
            }

            // Write via centralized writer
            await writer.WriteReleaseAsync(artistId, releaseFolderName, built);

            result.Success = true;
            result.ReleaseFolderPath = releaseFolderPath;
            result.ReleaseJson = built;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;

            // Cleanup on error
            if (Directory.Exists(releaseFolderPath))
            {
                try
                {
                    Directory.Delete(releaseFolderPath, true);
                }
                catch
                {
                    // Ignore cleanup errors
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
