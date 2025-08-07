using MusicGQL.Integration.Spotify;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Service for finding artist information on Spotify
/// </summary>
public class SpotifyImportService(SpotifyService spotifyService)
{
    /// <summary>
    /// Searches for an artist by name on Spotify
    /// </summary>
    /// <param name="artistName">Artist name to search for</param>
    /// <returns>List of matching Spotify artists</returns>
    public async Task<List<SpotifyArtistResult>> SearchArtistsAsync(string artistName)
    {
        try
        {
            var searchResults = await spotifyService.SearchArtistsAsync(artistName, 10);

            return searchResults
                .Select(artist => new SpotifyArtistResult
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    Genres = artist.Genres.ToList(),
                    Popularity = artist.Popularity,
                    Followers = artist.Followers.Total,
                    ExternalUrls = artist.ExternalUrls.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value
                    ),
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching Spotify for artist '{artistName}': {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets detailed artist information from Spotify by ID
    /// </summary>
    /// <param name="spotifyId">Spotify artist ID</param>
    /// <returns>Detailed artist information</returns>
    public async Task<SpotifyArtistResult?> GetArtistByIdAsync(string spotifyId)
    {
        try
        {
            var artist = await spotifyService.GetArtistAsync(spotifyId);

            if (artist == null)
                return null;

            return new SpotifyArtistResult
            {
                Id = artist.Id,
                Name = artist.Name,
                Genres = artist.Genres.ToList(),
                Popularity = artist.Popularity,
                Followers = artist.Followers.Total,
                ExternalUrls = artist.ExternalUrls.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching Spotify artist '{spotifyId}': {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Finds the best matching Spotify artist for a given artist name
    /// </summary>
    /// <param name="artistName">Artist name to match</param>
    /// <returns>Best matching Spotify artist or null</returns>
    public async Task<SpotifyArtistResult?> FindBestMatchAsync(string artistName)
    {
        var searchResults = await SearchArtistsAsync(artistName);

        if (!searchResults.Any())
            return null;

        // Find exact name match first
        var exactMatch = searchResults.FirstOrDefault(r =>
            string.Equals(r.Name, artistName, StringComparison.OrdinalIgnoreCase)
        );

        if (exactMatch != null)
            return exactMatch;

        // Otherwise return the most popular result
        return searchResults.OrderByDescending(r => r.Popularity).First();
    }
}

/// <summary>
/// Spotify artist search/lookup result
/// </summary>
public class SpotifyArtistResult
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = [];
    public int Popularity { get; set; }
    public int Followers { get; set; }
    public Dictionary<string, string> ExternalUrls { get; set; } = new();
}
