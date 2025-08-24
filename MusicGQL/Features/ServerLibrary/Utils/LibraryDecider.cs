using MbRelease = Hqub.MusicBrainz.Entities.Release;

namespace MusicGQL.Features.ServerLibrary.Utils;

public static class LibraryDecider
{
    private static readonly string[] NonStandardEditionKeywords = new[]
    {
        "deluxe",
        "anniversary",
        "expanded",
        "special",
        "bonus",
        "tour",
        "remaster",
        "remastered",
        "collector",
        "edition",
    };

    public static bool ShouldBeAddedWhenAddingArtistToServerLibrary(
        Hqub.MusicBrainz.Entities.ReleaseGroup releaseGroup
    ) => releaseGroup.IsMainAlbum() || releaseGroup.IsMainEP() || releaseGroup.IsMainSingle();

    public static MbRelease? GetMainReleaseInReleaseGroup(List<MbRelease> releases)
    {
        // Prefer an "ordinary" official release that represents the original album/ep/single
        var official = releases
            .Where(r => r.Status == "Official")
            // Exclude demos entirely
            .Where(r => r.ReleaseGroup != null && !r.ReleaseGroup.IsDemo())
            .ToList();

        if (official.Count == 0)
        {
            return null;
        }

        // Score each candidate
        MbRelease? best = null;
        int bestScore = int.MinValue;

        foreach (var r in official)
        {
            var countryPref = r.Country switch
            {
                "XW" => 300,
                "US" => 200,
                "GB" => 150,
                _ => 100,
            };

            // Track count heuristic: standard editions usually have fewer tracks than deluxe/anniversary
            var totalTracks =
                r.Media?.SelectMany(m => m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>())
                    .Count() ?? 0;
            var primaryType = r.ReleaseGroup?.PrimaryType ?? "";
            var idealMin = 0;
            var idealMax = 999;
            if (string.Equals(primaryType, "Album", StringComparison.OrdinalIgnoreCase))
            {
                idealMin = 8; // avoid tiny/partial releases
                idealMax = 20; // avoid 2xCD deluxe when selecting a canonical album
            }
            else if (string.Equals(primaryType, "EP", StringComparison.OrdinalIgnoreCase))
            {
                idealMin = 3;
                idealMax = 8;
            }
            else if (string.Equals(primaryType, "Single", StringComparison.OrdinalIgnoreCase))
            {
                idealMin = 1;
                idealMax = 4;
            }

            var withinIdeal = totalTracks >= idealMin && totalTracks <= idealMax;
            var trackScore = withinIdeal
                ? 300
                : Math.Max(0, 200 - Math.Abs(((idealMin + idealMax) / 2) - totalTracks) * 10);

            // Penalize likely non-standard editions by title keywords
            var title = r.Title ?? string.Empty;
            var hasNonStandardKeyword = NonStandardEditionKeywords.Any(k =>
                title.Contains(k, StringComparison.OrdinalIgnoreCase)
            );
            var keywordPenalty = hasNonStandardKeyword ? -400 : 0;

            // Prefer earlier releases
            // Dates in MusicBrainz are ISO strings (YYYY-MM-DD). We'll use string comparison with fallback
            int dateScore = 0;
            try
            {
                // earlier date => higher score (invert year offset)
                // If null/empty, neutral
                if (!string.IsNullOrWhiteSpace(r.Date))
                {
                    // Earlier dates should win, give a base and subtract months offset
                    dateScore = 0 - ParseDateOffset(r.Date!);
                }
            }
            catch { }

            var score = countryPref + trackScore + keywordPenalty + dateScore;

            if (score > bestScore)
            {
                bestScore = score;
                best = r;
            }
        }

        return best;
    }

    private static int ParseDateOffset(string date)
    {
        // Convert YYYY[-MM[-DD]] into a relative offset integer; larger means later
        // We return an integer that increases with later dates so earlier dates make smaller (more negative) dateScore above
        var parts = date.Split('-');
        int year = parts.Length > 0 && int.TryParse(parts[0], out var y) ? y : 9999;
        int month = parts.Length > 1 && int.TryParse(parts[1], out var m) ? m : 12;
        int day = parts.Length > 2 && int.TryParse(parts[2], out var d) ? d : 31;
        return year * 372 + month * 31 + day; // rough linearization
    }

    public static MbRelease? FindMainAlbumForSong(List<MbRelease> releases)
    {
        var official = releases.Where(r => r.Status == "Official").ToList();

        if (official.Count == 0)
        {
            return null;
        }

        var allAlbums = official.Where(r => r.ReleaseGroup.IsMainAlbum()).ToList();

        if (allAlbums.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allAlbums);
        }

        var allEps = official.Where(r => r.ReleaseGroup.IsMainEP()).ToList();

        if (allEps.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allEps);
        }

        var allSingles = official.Where(r => r.ReleaseGroup.IsMainSingle()).ToList();

        if (allSingles.Count > 0)
        {
            return FindPrioritizedRegionalAlbum(allSingles);
        }

        return official.FirstOrDefault();
    }

    public static MbRelease? FindPrioritizedRegionalAlbum(List<MbRelease> releases)
    {
        if (releases.Count == 1)
        {
            return releases.First();
        }

        return releases.LastOrDefault(a => a.Country == "XW")
            ?? releases.LastOrDefault(a => a.Country == "US")
            ?? releases.LastOrDefault(a => a.Country == "GB")
            ?? releases.LastOrDefault();
    }
}
