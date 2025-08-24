using MusicGQL.Features.ServerLibrary.Writer;
using MusicGQL.Features.ServerSettings;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary;

/// <summary>
/// Centralized service to assign media files on disk to release.json track AudioFilePath entries.
/// Sorting strategy:
/// 1) Prefer files with leading track number (e.g., 01-, 1., 01_, 01 ) and sort by that number
/// 2) Then fallback to case-insensitive filename sort
/// Only assigns the first N files to N tracks in order (by track number position in JSON)
/// </summary>
public class MediaFileAssignmentService(
    ServerLibraryJsonWriter writer,
    ServerSettingsAccessor serverSettingsAccessor
)
{
    public async Task<bool> AssignAsync(string artistId, string releaseFolderName)
    {
        var lib = await serverSettingsAccessor.GetAsync();
        var releaseDir = Path.Combine(lib.LibraryPath, artistId, releaseFolderName);
        var releaseJsonPath = Path.Combine(releaseDir, "release.json");
        if (!File.Exists(releaseJsonPath))
            return false;

        var audioExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3",
            ".flac",
            ".wav",
            ".m4a",
            ".ogg",
        };

        var files = Directory
            .GetFiles(releaseDir)
            .Where(f => audioExts.Contains(Path.GetExtension(f)))
            .Select(f => new
            {
                Full = f,
                Name = Path.GetFileName(f)!,
                Lead = ExtractLeadingTrackNumber(Path.GetFileNameWithoutExtension(f)!),
            })
            .OrderBy(x => x.Lead ?? int.MaxValue)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Name)
            .ToList();

        if (files.Count == 0)
            return false;

        await writer.UpdateReleaseAsync(
            artistId,
            releaseFolderName,
            rel =>
            {
                if (rel.Tracks == null)
                    return;
                for (int i = 0; i < rel.Tracks.Count; i++)
                {
                    if (i < files.Count)
                    {
                        rel.Tracks[i].AudioFilePath = "./" + files[i];
                    }
                }
            }
        );

        return true;
    }

    private static int? ExtractLeadingTrackNumber(string name)
    {
        // patterns: 01 -, 1., 1_, 1
        var span = name.AsSpan();
        int pos = 0;
        while (pos < span.Length && char.IsWhiteSpace(span[pos]))
            pos++;
        int start = pos;
        while (pos < span.Length && char.IsDigit(span[pos]))
            pos++;
        if (pos > start)
        {
            if (int.TryParse(span.Slice(start, pos - start), out var n))
                return n;
        }
        return null;
    }
}
