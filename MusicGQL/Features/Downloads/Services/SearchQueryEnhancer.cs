using ServerSettingsRecord = MusicGQL.Features.ServerSettings.ServerSettings;

namespace MusicGQL.Features.Downloads.Services;

/// <summary>
/// Service for enhancing search queries, particularly for releases with short titles
/// </summary>
public static class SearchQueryEnhancer
{
    // Minimum length threshold for considering a title "short"
    private const int ShortTitleThreshold = 3;

    // Contextual keywords to add for different types of releases
    private static readonly string[] ReleaseKeywords = ["album", "single", "ep", "LP"];
    private static readonly string[] MusicKeywords = ["music", "audio", "track", "song"];

    /// <summary>
    /// Enhances a search query if the release title is short and enhancement is enabled
    /// </summary>
    /// <param name="artistName">The artist name</param>
    /// <param name="releaseTitle">The release title</param>
    /// <param name="settings">Server settings to check if enhancement is enabled</param>
    /// <param name="logger">Logger for tracking enhancements</param>
    /// <param name="year">Optional release year for additional specificity</param>
    /// <returns>The enhanced query or original query if no enhancement needed</returns>
    public static string EnhanceQuery(
        string artistName,
        string releaseTitle,
        ServerSettingsRecord settings,
        ILogger logger,
        int? year = null
    )
    {
        var baseQuery = $"{artistName} {releaseTitle}".Trim();
        var enhancedQuery = baseQuery;

        if (!settings.SearchEnhanceShortTitles())
        {
            // Even if short title enhancement is disabled, add year if available
            if (year.HasValue)
            {
                enhancedQuery = $"{baseQuery} {year.Value}";
            }
            return enhancedQuery;
        }

        // Clean the release title for length checking (remove punctuation)
        var cleanTitle = releaseTitle
            .Replace("\"", "")
            .Replace("'", "")
            .Replace("-", "")
            .Replace(".", "")
            .Trim();

        if (cleanTitle.Length < ShortTitleThreshold)
        {
            // Determine what type of contextual keyword to add
            enhancedQuery = EnhanceShortTitle(artistName, releaseTitle, cleanTitle, logger);
            logger.LogInformation(
                "[SearchEnhancer] Enhanced short title '{Title}' → '{Enhanced}'",
                releaseTitle,
                enhancedQuery
            );
        }

        // Add year information if available for additional specificity
        if (year.HasValue)
        {
            enhancedQuery = $"{enhancedQuery} {year.Value}";
            logger.LogInformation("[SearchEnhancer] Added year {Year} to search query", year.Value);
        }

        return enhancedQuery;
    }

    /// <summary>
    /// Enhances a search query for a short release title by adding contextual keywords
    /// </summary>
    private static string EnhanceShortTitle(
        string artistName,
        string releaseTitle,
        string cleanTitle,
        ILogger logger
    )
    {
        // Strategy: Try different contextual keywords to make the search more specific

        // 1. Try release-type keywords first (album, single, ep, LP)
        foreach (var keyword in ReleaseKeywords)
        {
            var enhanced = $"{artistName} {releaseTitle} {keyword}".Trim();
            if (enhanced.Length > cleanTitle.Length + 5) // Make sure we're actually adding meaningful content
            {
                return enhanced;
            }
        }

        // 2. If release keywords don't add much value, try music keywords
        foreach (var keyword in MusicKeywords)
        {
            var enhanced = $"{artistName} {releaseTitle} {keyword}".Trim();
            if (enhanced.Length > cleanTitle.Length + 3)
            {
                return enhanced;
            }
        }

        // 3. Fallback: just add "album" as a default
        return $"{artistName} {releaseTitle} album".Trim();
    }

    /// <summary>
    /// Gets the length of a string after cleaning punctuation
    /// </summary>
    public static int GetCleanedLength(string text)
    {
        return text.Replace("\"", "")
            .Replace("'", "")
            .Replace("-", "")
            .Replace(".", "")
            .Trim()
            .Length;
    }
}
