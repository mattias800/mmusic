using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.Spotify;

namespace MusicGQL.Features.Import.Services.TopTracks;

public class TopTracksCompleter(SpotifyService spotifyService)
{
    private static string Normalize(string s) => (s ?? string.Empty).Trim().ToLowerInvariant();

    public async Task CompleteAsync(string artistDir, JsonArtist artist)
    {
        if (artist.TopTracks == null || artist.TopTracks.Count == 0)
        {
            return;
        }

        // Find Spotify artist id either from connections or via search by name
        string? spotifyArtistId = artist.Connections?.SpotifyId;
        if (string.IsNullOrWhiteSpace(spotifyArtistId))
        {
            try
            {
                var candidates = await spotifyService.SearchArtistsAsync(artist.Name, 1);
                var best = candidates?.FirstOrDefault();
                if (best != null)
                {
                    spotifyArtistId = best.Id;
                    artist.Connections ??= new JsonArtistServiceConnections();
                    artist.Connections.SpotifyId = spotifyArtistId;
                }
            }
            catch
            {
                // ignore spotify failures
            }
        }

        List<SpotifyAPI.Web.FullTrack>? spotifyTopTracks = null;
        if (!string.IsNullOrWhiteSpace(spotifyArtistId))
        {
            try
            {
                spotifyTopTracks =
                    await spotifyService.GetArtistTopTracksAsync(spotifyArtistId!) ?? [];
            }
            catch
            {
                spotifyTopTracks = [];
            }
        }

        var titleToSpotify =
            spotifyTopTracks
                ?.GroupBy(t => Normalize(t.Name))
                .ToDictionary(g => g.Key, g => g.First())
            ?? new Dictionary<string, SpotifyAPI.Web.FullTrack>();

        // Ensure an HttpClient for downloading images when needed
        using var httpClient = new HttpClient();

        for (int i = 0; i < artist.TopTracks.Count; i++)
        {
            var tt = artist.TopTracks[i];

            var normalizedTitle = Normalize(tt.Title);
            titleToSpotify.TryGetValue(normalizedTitle, out var spotifyMatch);

            // Fallback release title from Spotify if missing
            if (string.IsNullOrWhiteSpace(tt.ReleaseTitle) && spotifyMatch?.Album != null)
            {
                tt.ReleaseTitle = spotifyMatch.Album.Name;
            }

            // Fill missing track length from Spotify if available
            if ((tt.TrackLength == null || tt.TrackLength == 0) && spotifyMatch != null)
            {
                tt.TrackLength = spotifyMatch.DurationMs;
            }

            // Cover art handling
            if (string.IsNullOrWhiteSpace(tt.CoverArt))
            {
                // If mapped to a local release, this should already have been set by caller from release cover art.
                // If still missing, try Spotify album image as a fallback.
                var image = spotifyMatch?.Album?.Images?.FirstOrDefault();
                if (image != null && !string.IsNullOrWhiteSpace(image.Url))
                {
                    try
                    {
                        var bytes = await httpClient.GetByteArrayAsync(image.Url);
                        var fileName = $"toptrack{(i + 1).ToString("00")}.jpg";
                        var fullPath = System.IO.Path.Combine(artistDir, fileName);
                        await File.WriteAllBytesAsync(fullPath, bytes);
                        tt.CoverArt = "./" + fileName;
                    }
                    catch
                    {
                        // ignore download failures
                    }
                }
            }
        }
    }
}
