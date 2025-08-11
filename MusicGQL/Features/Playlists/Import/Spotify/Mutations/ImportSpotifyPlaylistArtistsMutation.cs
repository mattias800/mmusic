using MusicGQL.Features.Artists;
using MusicGQL.Features.Import;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Spotify;
using MusicGQL.Types;

namespace MusicGQL.Features.Playlists.Import.Spotify.Mutations;

[ExtendObjectType(typeof(Mutation))]
public class ImportSpotifyPlaylistArtistsMutation
{
    public async Task<ImportArtistsFromSpotifyPlaylistResult> ImportArtistsFromSpotifyPlaylist(
        ImportArtistsFromSpotifyPlaylistInput input,
        [Service] SpotifyService spotifyService,
        [Service] MusicBrainzService musicBrainzService,
        [Service] LibraryImportService libraryImportService,
        [Service] ServerLibraryCache cache
    )
    {
        var tracks = await spotifyService.GetTracksFromPlaylist(input.PlaylistId) ?? [];
        var uniqueArtistNames = tracks
            .SelectMany(t => t.Artists?.Select(a => a.Name) ?? [])
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var imported = 0;
        var importedArtistIds = new List<string>();
        var failed = new List<string>();

        foreach (var artistName in uniqueArtistNames)
        {
            try
            {
                // Strategy: search by artist name and pick best-scored result
                var candidates = await musicBrainzService.SearchArtistByNameAsync(
                    artistName,
                    10,
                    0
                );
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
                    if (!string.IsNullOrWhiteSpace(res.ArtistJson?.Id))
                    {
                        importedArtistIds.Add(res.ArtistJson!.Id!);
                    }
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

        var artists = new List<Artist>();
        foreach (var id in importedArtistIds.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var cached = await cache.GetArtistByIdAsync(id);
            if (cached != null)
            {
                artists.Add(new Artist(cached));
            }
        }

        return new ImportArtistsFromSpotifyPlaylistSuccess(
            artists,
            uniqueArtistNames.Count,
            imported,
            failed
        );
    }
}

public record ImportArtistsFromSpotifyPlaylistInput(string PlaylistId);

[UnionType("ImportArtistsFromSpotifyPlaylistResult")]
public abstract record ImportArtistsFromSpotifyPlaylistResult;

public record ImportArtistsFromSpotifyPlaylistSuccess(
    List<Artist> Artists,
    int TotalArtists,
    int ImportedArtists,
    List<string> FailedArtists
) : ImportArtistsFromSpotifyPlaylistResult;

public record ImportArtistsFromSpotifyPlaylistError(string Message)
    : ImportArtistsFromSpotifyPlaylistResult;
