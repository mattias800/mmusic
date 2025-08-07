using System.Text.Json;
using System.Text.Json.Serialization;
using MusicGQL.Features.Import.Services;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.MusicBrainz;
using Path = System.IO.Path;

namespace MusicGQL.Features.ServerLibrary.Reader;

/// <summary>
/// Scans the library folder for artist/release folders that contain audio files but
/// are missing their corresponding JSON metadata files. When found, imports metadata
/// from MusicBrainz and creates the JSON files in-place, then refreshes the cache.
/// </summary>
public class ServerLibraryFileSystemScanner(
    MusicBrainzService musicBrainzService,
    FanArtDownloadService fanArtDownloadService,
    ServerLibraryCache cache
)
{
    private const string LibraryPath = "./Library/";

    private static readonly string[] AudioExtensions = [".mp3", ".flac", ".wav", ".m4a", ".ogg"];

    public class ScanResult
    {
        public bool Success { get; set; }
        public int ArtistsCreated { get; set; }
        public int ReleasesCreated { get; set; }
        public List<string> Notes { get; set; } = [];
        public string? ErrorMessage { get; set; }
    }

    public async Task<ScanResult> ScanAndFixMissingMetadataAsync()
    {
        var result = new ScanResult();

        try
        {
            if (!Directory.Exists(LibraryPath))
            {
                result.Success = true;
                result.Notes.Add($"Library path not found: {Path.GetFullPath(LibraryPath)}");
                return result;
            }

            var artistDirectories = Directory.GetDirectories(LibraryPath);

            foreach (var artistDir in artistDirectories)
            {
                var artistFolderName = Path.GetFileName(artistDir) ?? "";
                var artistJsonPath = Path.Combine(artistDir, "artist.json");

                // Determine if there is at least one release folder with audio files
                var releaseDirs = Directory.GetDirectories(artistDir);
                var releaseDirsWithAudio = releaseDirs.Where(ContainsAnyAudioFile).ToList();

                if (releaseDirsWithAudio.Count == 0)
                {
                    // No audio files anywhere under this artist; skip entirely
                    continue;
                }

                string? artistMusicBrainzId = null;
                string artistDisplayName = artistFolderName;

                // Ensure artist.json exists
                if (!File.Exists(artistJsonPath))
                {
                    var mbArtists = await musicBrainzService.SearchArtistByNameAsync(
                        artistFolderName,
                        5
                    );

                    var mbArtist = mbArtists.FirstOrDefault();
                    if (mbArtist == null)
                    {
                        result.Notes.Add(
                            $"Artist not found on MusicBrainz: '{artistFolderName}', skipping artist"
                        );
                        continue;
                    }

                    artistDisplayName = mbArtist.Name ?? artistFolderName;
                    artistMusicBrainzId = mbArtist.Id;

                    // Download some photos (optional best-effort)
                    var photos = await fanArtDownloadService.DownloadArtistPhotosAsync(
                        artistMusicBrainzId,
                        artistDir
                    );

                    var jsonArtist = new JsonArtist
                    {
                        Id = artistFolderName,
                        Name = artistDisplayName,
                        SortName = mbArtist.SortName,
                        Photos = new JsonArtistPhotos
                        {
                            Thumbs = photos.Thumbs.Any() ? photos.Thumbs : null,
                            Backgrounds = photos.Backgrounds.Any() ? photos.Backgrounds : null,
                            Banners = photos.Banners.Any() ? photos.Banners : null,
                            Logos = photos.Logos.Any() ? photos.Logos : null,
                        },
                        Connections = new JsonArtistServiceConnections
                        {
                            MusicBrainzArtistId = artistMusicBrainzId,
                        },
                    };

                    var artistJson = JsonSerializer.Serialize(jsonArtist, GetJsonOptions());
                    await File.WriteAllTextAsync(artistJsonPath, artistJson);
                    result.ArtistsCreated++;
                    result.Notes.Add($"Created artist.json for '{artistFolderName}'");
                }
                else
                {
                    // Try to read artist.json for existing MBID
                    try
                    {
                        var json = await File.ReadAllTextAsync(artistJsonPath);
                        var parsed = JsonSerializer.Deserialize<JsonArtist>(json, GetJsonOptions());
                        artistMusicBrainzId = parsed?.Connections?.MusicBrainzArtistId;
                        artistDisplayName = parsed?.Name ?? artistDisplayName;
                    }
                    catch
                    {
                        // ignore parse errors; we'll operate without MBID
                    }
                }

                // For every release folder containing audio, ensure release.json exists
                foreach (var releaseDir in releaseDirsWithAudio)
                {
                    var releaseJsonPath = Path.Combine(releaseDir, "release.json");
                    if (File.Exists(releaseJsonPath))
                        continue;

                    var releaseFolderName = Path.GetFileName(releaseDir) ?? "";

                    // Try to find a matching release group on MusicBrainz
                    var mbReleaseGroups = await musicBrainzService.SearchReleaseGroupByNameAsync(
                        releaseFolderName,
                        10
                    );

                    var matchedRg = mbReleaseGroups
                        .Where(rg => rg != null)
                        .FirstOrDefault(rg =>
                            rg!.Credits?.Any() == true
                                ? rg.Credits.Any(ac =>
                                    string.Equals(
                                        ac.Name,
                                        artistDisplayName,
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                    || (
                                        artistMusicBrainzId != null
                                        && ac.Artist?.Id == artistMusicBrainzId
                                    )
                                )
                                : true
                        );

                    if (matchedRg == null)
                    {
                        result.Notes.Add(
                            $"Could not match release '{releaseFolderName}' for artist '{artistDisplayName}' on MusicBrainz"
                        );
                        continue;
                    }

                    // Fetch releases with recordings for the group and pick one
                    var releases = await musicBrainzService.GetReleasesForReleaseGroupAsync(
                        matchedRg.Id
                    );
                    var selected = releases.FirstOrDefault();

                    // Download cover art if possible
                    string? coverArtRelPath =
                        await fanArtDownloadService.DownloadReleaseCoverArtAsync(
                            matchedRg.Id,
                            releaseDir
                        );

                    var releaseType = matchedRg.PrimaryType?.ToLowerInvariant() switch
                    {
                        "album" => JsonReleaseType.Album,
                        "ep" => JsonReleaseType.Ep,
                        "single" => JsonReleaseType.Single,
                        _ => JsonReleaseType.Album,
                    };

                    var tracks = selected
                        ?.Media?.SelectMany(m =>
                            m.Tracks ?? new List<Hqub.MusicBrainz.Entities.Track>()
                        )
                        .Select(t => new JsonTrack
                        {
                            Title = t.Recording?.Title ?? "",
                            TrackNumber = t.Position,
                            TrackLength = t.Length,
                            AudioFilePath = null,
                        })
                        .Where(t => t.TrackNumber > 0)
                        .OrderBy(t => t.TrackNumber)
                        .ToList();

                    var jsonRelease = new JsonRelease
                    {
                        Title = matchedRg.Title ?? releaseFolderName,
                        SortTitle = matchedRg.Title,
                        Type = releaseType,
                        FirstReleaseDate = matchedRg.FirstReleaseDate,
                        FirstReleaseYear =
                            matchedRg.FirstReleaseDate?.Length >= 4
                                ? matchedRg.FirstReleaseDate!.Substring(0, 4)
                                : null,
                        CoverArt = coverArtRelPath,
                        Tracks = tracks?.Count > 0 ? tracks : null,
                    };

                    var jsonText = JsonSerializer.Serialize(jsonRelease, GetJsonOptions());
                    await File.WriteAllTextAsync(releaseJsonPath, jsonText);
                    result.ReleasesCreated++;
                    result.Notes.Add(
                        $"Created release.json for '{artistFolderName}/{releaseFolderName}'"
                    );
                }
            }

            // Refresh cache if we created anything
            if (result.ArtistsCreated > 0 || result.ReleasesCreated > 0)
            {
                await cache.UpdateCacheAsync();
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
    }

    private static bool ContainsAnyAudioFile(string directory)
    {
        if (!Directory.Exists(directory))
            return false;

        try
        {
            // only scan immediate files within the release folder
            var files = Directory.GetFiles(directory);
            return files.Any(f =>
                AudioExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())
            );
        }
        catch
        {
            return false;
        }
    }
}
