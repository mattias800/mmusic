namespace MusicGQL.Features.External.Downloads.Prowlarr;

internal static class ProwlarrResultFilter
{
    public static bool IsValidMusicResult(ProwlarrRelease release, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return false;

        var title = release.Title.ToLowerInvariant();
        var artist = artistName.ToLowerInvariant();
        var album = releaseTitle.ToLowerInvariant();

        if (ContainsNonMusicTerms(title))
            return false;

        if (!ContainsArtistName(title, artist))
            return false;

        if (!ContainsAlbumTitle(title, album))
            return false;

        return true;
    }

    public static string GetRejectionReason(ProwlarrRelease release, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return "Missing title";

        var title = release.Title.ToLowerInvariant();
        var artist = artistName.ToLowerInvariant();
        var album = releaseTitle.ToLowerInvariant();

        if (ContainsNonMusicTerms(title))
            return "Contains non-music terms (TV/movie/video)";

        if (!ContainsArtistName(title, artist))
            return "Artist name mismatch";

        if (!ContainsAlbumTitle(title, album))
            return "Album title mismatch";

        if (ProwlarrScorer.IsTorrentFile(release.DownloadUrl))
            return "Torrent file (not compatible with SABnzbd)";

        return "Unknown validation failure";
    }

    private static bool ContainsNonMusicTerms(string title)
    {
        var nonMusicTerms = new[]
        {
            "1080p", "720p", "4k", "hdtv", "web-dl", "bluray", "dvdrip", "h264", "h265", "x264", "x265",
            "season", "episode", "s01", "s02", "e01", "e02", "complete", "series",
            "movie", "film", "documentary", "show", "tv", "television",
            "subtitle", "dub", "dubbed", "multi", "dual", "audio"
        };

        return nonMusicTerms.Any(term => title.Contains(term));
    }

    private static bool ContainsArtistName(string title, string artistName)
    {
        // Simple fuzzy: require artist token containment or close similarity on at least half of words
        var artistWords = artistName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matchingWords = artistWords.Count(word => titleWords.Any(titleWord => ProwlarrTextMatch.FuzzyWordMatch(titleWord, word)));
        return matchingWords >= Math.Max(1, artistWords.Length / 2);
    }

    private static bool ContainsAlbumTitle(string title, string albumTitle)
    {
        return ProwlarrTextMatch.ContainsAlbumTitleFuzzy(title, albumTitle);
    }
}

