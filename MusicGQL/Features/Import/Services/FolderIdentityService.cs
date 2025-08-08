using MusicGQL.Integration.MusicBrainz;
using Path = System.IO.Path;

namespace MusicGQL.Features.Import.Services;

public interface IFolderIdentityService
{
    Task<IdentifyArtistResult?> IdentifyArtistAsync(
        string artistDir,
        IEnumerable<string> releaseDirs
    );
    Task<IdentifyReleaseResult?> IdentifyReleaseAsync(
        string artistName,
        string? mbArtistId,
        string releaseDir
    );
}

public sealed class FolderIdentityService(MusicBrainzService musicBrainzService)
    : IFolderIdentityService
{
    public async Task<IdentifyArtistResult?> IdentifyArtistAsync(
        string artistDir,
        IEnumerable<string> releaseDirs
    )
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
        var releaseNames = releaseDirs
            .Select(Path.GetFileName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();
        foreach (var candidate in candidates)
        {
            try
            {
                var rgs = await musicBrainzService.GetReleaseGroupsForArtistAsync(candidate.Id);
                var rgTitles = rgs.Select(r => r.Title?.ToLowerInvariant())
                    .Where(t => t != null)
                    .ToHashSet();
                if (releaseNames.Any(r => rgTitles.Contains(r!.ToLowerInvariant())))
                {
                    return new IdentifyArtistResult(
                        candidate.Id,
                        candidate.Name ?? artistFolderName
                    );
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

    public async Task<IdentifyReleaseResult?> IdentifyReleaseAsync(
        string artistName,
        string? mbArtistId,
        string releaseDir
    )
    {
        var releaseFolderName = Path.GetFileName(releaseDir) ?? string.Empty;
        var groups = await musicBrainzService.SearchReleaseGroupByNameAsync(releaseFolderName, 10);

        // Count local audio files to help pick the correct release group
        var audioExtensions = new[] { ".mp3", ".flac", ".wav", ".m4a", ".ogg" };
        var audioFileCount = Directory
            .GetFiles(releaseDir)
            .Count(f => audioExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

        IdentifyReleaseResult? best = null;
        int bestScore = int.MinValue;

        foreach (var g in groups.Where(g => g != null))
        {
            // Ensure artist matches this release group when credits are available
            if (g!.Credits?.Any() == true)
            {
                var hasArtist = g.Credits.Any(ac =>
                    string.Equals(ac.Name, artistName, StringComparison.OrdinalIgnoreCase)
                    || (mbArtistId != null && ac.Artist?.Id == mbArtistId)
                );
                if (!hasArtist)
                    continue;
            }

            // Fetch releases for the group to evaluate track counts
            List<Hqub.MusicBrainz.Entities.Release> releases;
            try
            {
                releases = await musicBrainzService.GetReleasesForReleaseGroupAsync(g.Id);
            }
            catch
            {
                continue;
            }

            var maxTracks = releases
                .Select(r =>
                    r.Media?.SelectMany(m =>
                            m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>()
                        )
                        .Count() ?? 0
                )
                .DefaultIfEmpty(0)
                .Max();

            var isAlbum = string.Equals(g.PrimaryType, "Album", StringComparison.OrdinalIgnoreCase);
            var titleExact = string.Equals(
                g.Title,
                releaseFolderName,
                StringComparison.OrdinalIgnoreCase
            );

            int score = 0;
            if (isAlbum)
                score += 1000;
            if (maxTracks == audioFileCount && audioFileCount > 0)
                score += 10000;
            score += Math.Max(0, 100 - Math.Abs(maxTracks - audioFileCount));
            if (titleExact)
                score += 50;

            if (score > bestScore)
            {
                bestScore = score;
                best = new IdentifyReleaseResult(g.Id, g.Title ?? releaseFolderName, g.PrimaryType);
            }
        }

        return best;
    }
}

public record IdentifyArtistResult(string MusicBrainzArtistId, string ArtistDisplayName);

public record IdentifyReleaseResult(string ReleaseGroupId, string Title, string? PrimaryType);
