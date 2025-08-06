using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.ServerLibrary2.Json;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary2.Reader;

public class ServerLibraryJsonReader
{
    private const string libraryPath = "./Library/";
    private readonly JsonSerializerOptions _jsonOptions;

    public ServerLibraryJsonReader()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }

    /// <summary>
    /// Reads all artists from the library
    /// </summary>
    /// <returns>List of artists with their metadata</returns>
    public async Task<List<(string ArtistPath, ArtistJson Artist)>> ReadAllArtistsAsync()
    {
        var artists = new List<(string, ArtistJson)>();
        
        if (!Directory.Exists(libraryPath))
        {
            return artists;
        }

        var artistDirectories = Directory.GetDirectories(libraryPath);
        
        foreach (var artistDir in artistDirectories)
        {
            var artistJsonPath = Path.Combine(artistDir, "artist.json");
            if (File.Exists(artistJsonPath))
            {
                try
                {
                    var artistJson = await ReadArtistFromPathAsync(artistDir);
                    if (artistJson != null)
                    {
                        artists.Add((artistDir, artistJson));
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with other artists
                    Console.WriteLine($"Error reading artist from {artistDir}: {ex.Message}");
                }
            }
        }

        return artists;
    }

    /// <summary>
    /// Reads all albums/releases for a specified artist by artist path
    /// </summary>
    /// <param name="artistPath">Path to the artist folder</param>
    /// <returns>List of releases with their metadata</returns>
    public async Task<List<(string ReleasePath, ReleaseJson Release)>> ReadArtistAlbumsAsync(string artistPath)
    {
        var releases = new List<(string, ReleaseJson)>();
        
        if (!Directory.Exists(artistPath))
        {
            return releases;
        }

        var releaseDirectories = Directory.GetDirectories(artistPath);
        
        foreach (var releaseDir in releaseDirectories)
        {
            var releaseJsonPath = Path.Combine(releaseDir, "release.json");
            if (File.Exists(releaseJsonPath))
            {
                try
                {
                    var releaseJson = await ReadReleaseFromPathAsync(releaseDir);
                    if (releaseJson != null)
                    {
                        releases.Add((releaseDir, releaseJson));
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with other releases
                    Console.WriteLine($"Error reading release from {releaseDir}: {ex.Message}");
                }
            }
        }

        return releases;
    }

    /// <summary>
    /// Reads all albums/releases for a specified artist by artist name
    /// </summary>
    /// <param name="artistName">Name of the artist</param>
    /// <returns>List of releases with their metadata</returns>
    public async Task<List<(string ReleasePath, ReleaseJson Release)>> ReadArtistAlbumsByNameAsync(string artistName)
    {
        var artistPath = Path.Combine(libraryPath, artistName);
        return await ReadArtistAlbumsAsync(artistPath);
    }

    /// <summary>
    /// Reads a single artist from the specified path
    /// </summary>
    /// <param name="artistPath">Path to the artist folder</param>
    /// <returns>Artist metadata or null if not found</returns>
    public async Task<ArtistJson?> ReadArtistFromPathAsync(string artistPath)
    {
        var artistJsonPath = Path.Combine(artistPath, "artist.json");
        
        if (!File.Exists(artistJsonPath))
        {
            return null;
        }

        var jsonContent = await File.ReadAllTextAsync(artistJsonPath);
        return JsonSerializer.Deserialize<ArtistJson>(jsonContent, _jsonOptions);
    }

    /// <summary>
    /// Reads a single artist by name
    /// </summary>
    /// <param name="artistName">Name of the artist</param>
    /// <returns>Artist metadata or null if not found</returns>
    public async Task<ArtistJson?> ReadArtistByNameAsync(string artistName)
    {
        var artistPath = Path.Combine(libraryPath, artistName);
        return await ReadArtistFromPathAsync(artistPath);
    }

    /// <summary>
    /// Reads a single release from the specified path
    /// </summary>
    /// <param name="releasePath">Path to the release folder</param>
    /// <returns>Release metadata or null if not found</returns>
    public async Task<ReleaseJson?> ReadReleaseFromPathAsync(string releasePath)
    {
        var releaseJsonPath = Path.Combine(releasePath, "release.json");
        
        if (!File.Exists(releaseJsonPath))
        {
            return null;
        }

        var jsonContent = await File.ReadAllTextAsync(releaseJsonPath);
        return JsonSerializer.Deserialize<ReleaseJson>(jsonContent, _jsonOptions);
    }

    /// <summary>
    /// Reads a single release by artist name and release name
    /// </summary>
    /// <param name="artistName">Name of the artist</param>
    /// <param name="releaseName">Name of the release</param>
    /// <returns>Release metadata or null if not found</returns>
    public async Task<ReleaseJson?> ReadReleaseAsync(string artistName, string releaseName)
    {
        var releasePath = Path.Combine(libraryPath, artistName, releaseName);
        return await ReadReleaseFromPathAsync(releasePath);
    }

    /// <summary>
    /// Gets all artist names in the library
    /// </summary>
    /// <returns>List of artist folder names</returns>
    public List<string> GetArtistNames()
    {
        if (!Directory.Exists(libraryPath))
        {
            return new List<string>();
        }

        return Directory.GetDirectories(libraryPath)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Cast<string>()
            .ToList();
    }

    /// <summary>
    /// Gets all release names for a specific artist
    /// </summary>
    /// <param name="artistName">Name of the artist</param>
    /// <returns>List of release folder names</returns>
    public List<string> GetReleaseNames(string artistName)
    {
        var artistPath = Path.Combine(libraryPath, artistName);
        
        if (!Directory.Exists(artistPath))
        {
            return new List<string>();
        }

        return Directory.GetDirectories(artistPath)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Cast<string>()
            .ToList();
    }

    /// <summary>
    /// Checks if an artist exists in the library
    /// </summary>
    /// <param name="artistName">Name of the artist</param>
    /// <returns>True if artist exists</returns>
    public bool ArtistExists(string artistName)
    {
        var artistPath = Path.Combine(libraryPath, artistName);
        var artistJsonPath = Path.Combine(artistPath, "artist.json");
        return File.Exists(artistJsonPath);
    }

    /// <summary>
    /// Checks if a release exists for an artist
    /// </summary>
    /// <param name="artistName">Name of the artist</param>
    /// <param name="releaseName">Name of the release</param>
    /// <returns>True if release exists</returns>
    public bool ReleaseExists(string artistName, string releaseName)
    {
        var releasePath = Path.Combine(libraryPath, artistName, releaseName);
        var releaseJsonPath = Path.Combine(releasePath, "release.json");
        return File.Exists(releaseJsonPath);
    }
}
