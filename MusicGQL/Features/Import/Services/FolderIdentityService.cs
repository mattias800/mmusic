using System.Linq;
using MusicGQL.Integration.MusicBrainz;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public interface IFolderIdentityService
{
    Task<IdentifyArtistResult?> IdentifyArtistAsync(string artistDir, IEnumerable<string> releaseDirs);
    Task<IdentifyReleaseResult?> IdentifyReleaseAsync(string artistName, string? mbArtistId, string releaseDir);
}

public sealed class FolderIdentityService(MusicBrainzService musicBrainzService) : IFolderIdentityService
{
    public async Task<IdentifyArtistResult?> IdentifyArtistAsync(string artistDir, IEnumerable<string> releaseDirs)
    {
        var artistFolderName = Path.GetFileName(artistDir) ?? string.Empty;
        var candidates = await musicBrainzService.SearchArtistByNameAsync(artistFolderName, 10);
        if (candidates.Count() == 0)
        {
            return null;
        }

        if (candidates.Count() == 1)
        {
            var single = candidates[0];
            return new IdentifyArtistResult(single.Id, single.Name ?? artistFolderName);
        }

        // Try disambiguation using releases
        var releaseNames = releaseDirs.Select(Path.GetFileName).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
        foreach (var candidate in candidates)
        {
            try
            {
                var rgs = await musicBrainzService.GetReleaseGroupsForArtistAsync(candidate.Id);
                var rgTitles = rgs.Select(r => r.Title?.ToLowerInvariant()).Where(t => t != null).ToHashSet();
                if (releaseNames.Any(r => rgTitles.Contains(r!.ToLowerInvariant())))
                {
                    return new IdentifyArtistResult(candidate.Id, candidate.Name ?? artistFolderName);
                }
            }
            catch
            {
                // ignore failures per candidate
            }
        }

        // Fallback to the top candidate by score
        var first = candidates.First();
        return new IdentifyArtistResult(first.Id, first.Name ?? artistFolderName);
    }

    public async Task<IdentifyReleaseResult?> IdentifyReleaseAsync(string artistName, string? mbArtistId, string releaseDir)
    {
        var releaseFolderName = Path.GetFileName(releaseDir) ?? string.Empty;
        var groups = await musicBrainzService.SearchReleaseGroupByNameAsync(releaseFolderName, 10);
        var matched = groups
            .Where(g => g != null)
            .FirstOrDefault(g =>
                g!.Credits?.Any() == true
                    ? g.Credits.Any(ac =>
                        string.Equals(ac.Name, artistName, StringComparison.OrdinalIgnoreCase)
                        || (mbArtistId != null && ac.Artist?.Id == mbArtistId)
                    )
                    : true
            );

        if (matched == null)
        {
            return null;
        }

        return new IdentifyReleaseResult(matched.Id, matched.Title ?? releaseFolderName, matched.PrimaryType);
    }
}

public record IdentifyArtistResult(string MusicBrainzArtistId, string ArtistDisplayName);

public record IdentifyReleaseResult(string ReleaseGroupId, string Title, string? PrimaryType);


