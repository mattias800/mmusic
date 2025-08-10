using MusicGQL.Features.Import;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Spotify;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Import.Spotify.Mutations;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class ImportSpotifyPlaylistArtistsMutation
{
    public async Task<ImportArtistsFromSpotifyPlaylistResult> ImportArtistsFromSpotifyPlaylist(
        [Service] SpotifyService spotifyService,
        [Service] MusicBrainzService musicBrainzService,
        [Service] LibraryImportService libraryImportService,
        string playlistId
    )
    {
        var tracks = await spotifyService.GetTracksFromPlaylist(playlistId) ?? [];
        var uniqueArtistNames = tracks
            .SelectMany(t => t.Artists?.Select(a => a.Name) ?? [])
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var imported = 0;
        var failed = new List<string>();

        foreach (var artistName in uniqueArtistNames)
        {
            try
            {
                // Strategy: search by artist name and pick best-scored result
                var candidates = await musicBrainzService.SearchArtistByNameAsync(artistName, 10, 0);
                var mbArtist = candidates.FirstOrDefault();
                if (mbArtist is null)
                {
                    failed.Add($"No MB match: {artistName}");
                    continue;
                }

                var res = await libraryImportService.ImportArtistByMusicBrainzIdAsync(mbArtist.Id);
                if (string.IsNullOrWhiteSpace(res.ErrorMessage))
                {
                    imported++;
                }
                else
                {
                    failed.Add($"{artistName}: {res.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                failed.Add($"{artistName}: {ex.Message}");
            }
        }

        return new ImportArtistsFromSpotifyPlaylistResult
        {
            Success = true,
            TotalArtists = uniqueArtistNames.Count,
            ImportedArtists = imported,
            FailedArtists = failed,
        };
    }
}

public class ImportArtistsFromSpotifyPlaylistResult
{
    public bool Success { get; set; }
    public int TotalArtists { get; set; }
    public int ImportedArtists { get; set; }
    public List<string> FailedArtists { get; set; } = new();
}


