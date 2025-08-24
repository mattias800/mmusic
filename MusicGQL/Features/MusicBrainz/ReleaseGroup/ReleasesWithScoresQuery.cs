using MusicGQL.Features.ServerSettings;
using MusicGQL.Integration.MusicBrainz;
using Path = System.IO.Path;

namespace MusicGQL.Features.MusicBrainz.ReleaseGroup;

public class ScoredRelease
{
    public Release.MbRelease Release { get; set; }
    public int Score { get; set; }
    public IEnumerable<string> Reasons { get; set; } = Array.Empty<string>();
}

[ExtendObjectType(typeof(Types.Query))]
public class ReleasesWithScoresQuery
{
    public async Task<IEnumerable<ScoredRelease>> ReleasesWithScores(
        [Service] MusicBrainzService mbService,
        [Service] ServerSettingsAccessor serverSettingsAccessor,
        string releaseGroupId,
        string artistId,
        string releaseFolderName
    )
    {
        var releases = await mbService.GetReleasesForReleaseGroupAsync(releaseGroupId);

        var releaseDir = Path.Combine(
            (await serverSettingsAccessor.GetAsync()).LibraryPath,
            artistId,
            releaseFolderName
        );
        var audioExtensions = new[] { ".mp3", ".flac", ".wav", ".m4a", ".ogg" };
        var audioFiles = Directory.Exists(releaseDir)
            ? Directory
                .GetFiles(releaseDir)
                .Where(f => audioExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList()
            : new List<string>();
        var audioFileCount = audioFiles.Count;

        var scored = new List<ScoredRelease>();
        foreach (var r in releases)
        {
            var reasons = new List<string>();
            int score = 0;

            var mediaTracks =
                r.Media?.SelectMany(m => m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>())
                    .Count() ?? 0;
            if (audioFileCount > 0)
            {
                if (mediaTracks == audioFileCount)
                {
                    score += 10000;
                    reasons.Add("Exact track count match");
                }
                score += Math.Max(0, 100 - Math.Abs(mediaTracks - audioFileCount));

                var titles =
                    r.Media?.SelectMany(m =>
                            m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>()
                        )
                        .Select(tt => tt.Recording?.Title ?? string.Empty)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(Normalize)
                        .ToList() ?? new List<string>();

                var filesNorm = audioFiles
                    .Select(f => Normalize(Path.GetFileNameWithoutExtension(f)))
                    .ToList();

                int matchCount = 0;
                int considered = Math.Min(Math.Min(titles.Count, filesNorm.Count), 30);
                for (int i = 0; i < considered; i++)
                {
                    var t = titles[i];
                    var f = filesNorm[i];
                    if (t.Length == 0 || f.Length == 0)
                        continue;
                    if (
                        f.Contains(t, StringComparison.OrdinalIgnoreCase)
                        || t.Contains(f, StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        matchCount++;
                    }
                    else
                    {
                        var tw = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var fw = f.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var overlap = tw.Intersect(fw, StringComparer.OrdinalIgnoreCase).Count();
                        if (overlap >= 2)
                            matchCount++;
                    }
                }
                if (matchCount > 0)
                    reasons.Add($"{matchCount} filename/title matches");
                score += matchCount * 50;
            }

            var title = r.Title ?? string.Empty;
            if (
                title.Contains("deluxe", StringComparison.OrdinalIgnoreCase)
                || title.Contains("anniversary", StringComparison.OrdinalIgnoreCase)
                || title.Contains("expanded", StringComparison.OrdinalIgnoreCase)
                || title.Contains("remaster", StringComparison.OrdinalIgnoreCase)
                || title.Contains("special", StringComparison.OrdinalIgnoreCase)
                || title.Contains("bonus", StringComparison.OrdinalIgnoreCase)
                || title.Contains("tour", StringComparison.OrdinalIgnoreCase)
            )
            {
                score -= 500;
                reasons.Add("Penalized: deluxe/anniversary/expanded");
            }

            if (string.Equals(r.Country, "US", StringComparison.OrdinalIgnoreCase))
            {
                score += 100;
                reasons.Add("Country: US");
            }
            if (string.Equals(r.Country, "XW", StringComparison.OrdinalIgnoreCase))
            {
                score += 150;
                reasons.Add("Country: Worldwide");
            }
            if (
                string.Equals(
                    r.ReleaseGroup?.PrimaryType,
                    "Album",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                score += 1000;
                reasons.Add("Type: Album");
            }

            scored.Add(
                new ScoredRelease
                {
                    Release = new Release.MbRelease(r),
                    Score = score,
                    Reasons = reasons,
                }
            );
        }

        return scored.OrderByDescending(s => s.Score).ToList();
    }

    private static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return string.Empty;
        var cleaned = new string(
            s.ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : ' ').ToArray()
        );
        var parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts);
    }
}
