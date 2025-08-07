using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Import.Services;

/// <summary>
/// Service for importing artist and release data from MusicBrainz
/// </summary>
public class MusicBrainzImportService(MusicBrainzService musicBrainzService)
{
    /// <summary>
    /// Searches for an artist by name on MusicBrainz
    /// </summary>
    /// <param name="artistName">Artist name to search for</param>
    /// <returns>List of matching artists with their MusicBrainz IDs</returns>
    public async Task<List<MusicBrainzArtistResult>> SearchArtistsAsync(string artistName)
    {
        try
        {
            var searchResults = await musicBrainzService.SearchArtistByNameAsync(artistName, 10);

            return searchResults
                .Select(artist => new MusicBrainzArtistResult
                {
                    Id = artist.Id,
                    Name = artist.Name,
                    SortName = artist.SortName,
                    Type = artist.Type,
                    Country = artist.Country,
                    BeginDate = artist.LifeSpan?.Begin,
                    EndDate = artist.LifeSpan?.End,
                    Tags = artist.Tags?.Select(t => t.Name).ToList() ?? [],
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error searching MusicBrainz for artist '{artistName}': {ex.Message}"
            );
            return [];
        }
    }

    /// <summary>
    /// Gets detailed artist information from MusicBrainz by ID
    /// </summary>
    /// <param name="musicBrainzId">MusicBrainz artist ID</param>
    /// <returns>Detailed artist information</returns>
    public async Task<MusicBrainzArtistResult?> GetArtistByIdAsync(string musicBrainzId)
    {
        try
        {
            var artist = await musicBrainzService.GetArtistByIdAsync(musicBrainzId);

            if (artist == null)
                return null;

            return new MusicBrainzArtistResult
            {
                Id = artist.Id,
                Name = artist.Name,
                SortName = artist.SortName,
                Type = artist.Type,
                Country = artist.Country,
                BeginDate = artist.LifeSpan?.Begin,
                EndDate = artist.LifeSpan?.End,
                Tags = artist.Tags?.Select(t => t.Name).ToList() ?? [],
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching MusicBrainz artist '{musicBrainzId}': {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets release groups for an artist from MusicBrainz
    /// </summary>
    /// <param name="musicBrainzId">MusicBrainz artist ID</param>
    /// <returns>List of release groups for the artist</returns>
    public async Task<List<MusicBrainzReleaseGroupResult>> GetArtistReleaseGroupsAsync(
        string musicBrainzId
    )
    {
        try
        {
            var releaseGroups = await musicBrainzService.GetReleaseGroupsForArtistAsync(
                musicBrainzId
            );

            return releaseGroups
                .Select(rg => new MusicBrainzReleaseGroupResult
                {
                    Id = rg.Id,
                    Title = rg.Title,
                    PrimaryType = rg.PrimaryType,
                    SecondaryTypes = rg.SecondaryTypes?.ToList() ?? [],
                    FirstReleaseDate = rg.FirstReleaseDate,
                    Tags = rg.Tags?.Select(t => t.Name).ToList() ?? [],
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error fetching release groups for artist '{musicBrainzId}': {ex.Message}"
            );
            return [];
        }
    }

    /// <summary>
    /// Gets releases for a release group with track information
    /// </summary>
    /// <param name="releaseGroupId">MusicBrainz release group ID</param>
    /// <returns>List of releases with tracks</returns>
    public async Task<List<MusicBrainzReleaseResult>> GetReleaseGroupReleasesAsync(
        string releaseGroupId
    )
    {
        try
        {
            var releases = await musicBrainzService.GetReleasesForReleaseGroupAsync(releaseGroupId);

            var releaseResults = new List<MusicBrainzReleaseResult>();

            foreach (var release in releases)
            {
                // Get detailed release info with tracks
                var detailedRelease = await musicBrainzService.GetReleaseByIdAsync(release.Id);
                if (detailedRelease?.Media == null)
                    continue;

                var tracks = detailedRelease
                    .Media.SelectMany(medium =>
                        medium.Tracks?.Select(track => new MusicBrainzTrackResult
                        {
                            Id = track.Id,
                            Title = track.Recording.Title,
                            TrackNumber = track.Position,
                            Length = track.Length,
                        }) ?? []
                    )
                    .ToList();

                releaseResults.Add(
                    new MusicBrainzReleaseResult
                    {
                        Id = release.Id,
                        Title = release.Title,
                        Date = release.Date,
                        Country = release.Country,
                        Status = release.Status,
                        Tracks = tracks,
                    }
                );
            }

            return releaseResults;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error fetching releases for release group '{releaseGroupId}': {ex.Message}"
            );
            return [];
        }
    }
}

/// <summary>
/// MusicBrainz artist search/lookup result
/// </summary>
public class MusicBrainzArtistResult
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SortName { get; set; }
    public string? Type { get; set; }
    public string? Country { get; set; }
    public string? BeginDate { get; set; }
    public string? EndDate { get; set; }
    public List<string> Tags { get; set; } = [];
}

/// <summary>
/// MusicBrainz release group result
/// </summary>
public class MusicBrainzReleaseGroupResult
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? PrimaryType { get; set; }
    public List<string> SecondaryTypes { get; set; } = [];
    public string? FirstReleaseDate { get; set; }
    public List<string> Tags { get; set; } = [];
}

/// <summary>
/// MusicBrainz release result
/// </summary>
public class MusicBrainzReleaseResult
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Date { get; set; }
    public string? Country { get; set; }
    public string? Status { get; set; }
    public List<MusicBrainzTrackResult> Tracks { get; set; } = [];
}

/// <summary>
/// MusicBrainz track result
/// </summary>
public class MusicBrainzTrackResult
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int TrackNumber { get; set; }
    public int? Length { get; set; }
}
