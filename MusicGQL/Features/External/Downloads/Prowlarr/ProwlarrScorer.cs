namespace MusicGQL.Features.External.Downloads.Prowlarr;

internal static class ProwlarrScorer
{
    public static bool IsTorrentFile(string? downloadUrl)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl))
            return false;

        return downloadUrl.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase)
            || downloadUrl.Contains("torrent", StringComparison.OrdinalIgnoreCase);
    }

    public static int CalculateRelevanceScore(
        ProwlarrRelease release,
        string artistName,
        string releaseTitle
    )
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return 0;

        var title = release.Title.ToLowerInvariant();
        var artist = artistName.ToLowerInvariant();
        var album = releaseTitle.ToLowerInvariant();

        int score = 0;

        // Base score for being valid
        score += 10;

        // Bonus for exact artist name match
        if (title.Contains(artist))
            score += 20;

        // Bonus for exact album title match
        if (title.Contains(album))
            score += 20;

        // Bonus for quality indicators
        var qualityTerms = new[] { "flac", "lossless", "320", "cd", "vinyl", "24bit", "16bit" };
        foreach (var term in qualityTerms)
        {
            if (title.Contains(term))
                score += 5;
        }

        // Bonus for common music file extensions
        var musicExtensions = new[] { ".mp3", ".flac", ".m4a", ".wav", ".ogg", ".aac" };
        foreach (var ext in musicExtensions)
        {
            if (title.Contains(ext))
                score += 3;
        }

        // Penalty for torrent files
        if (IsTorrentFile(release.DownloadUrl))
            score -= 50;

        return score;
    }
}
