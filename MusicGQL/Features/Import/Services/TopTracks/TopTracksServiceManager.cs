using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerSettings;
using Microsoft.Extensions.Logging;

namespace MusicGQL.Features.Import.Services.TopTracks;

public class TopTracksServiceManager
{
    private readonly ITopTracksImporter _listenBrainzImporter;
    private readonly ITopTracksImporter _spotifyImporter;
    private readonly ITopTracksImporter _lastFmImporter;
    private readonly ServerSettingsAccessor _serverSettingsAccessor;
    private readonly ILogger<TopTracksServiceManager> _logger;

    public TopTracksServiceManager(
        TopTracksListenBrainzImporter listenBrainzImporter,
        TopTracksSpotifyImporter spotifyImporter,
        TopTracksLastFmImporter lastFmImporter,
        ServerSettingsAccessor serverSettingsAccessor,
        ILogger<TopTracksServiceManager> logger)
    {
        _listenBrainzImporter = listenBrainzImporter;
        _spotifyImporter = spotifyImporter;
        _lastFmImporter = lastFmImporter;
        _serverSettingsAccessor = serverSettingsAccessor;
        _logger = logger;
    }

    public async Task<List<JsonTopTrack>> GetTopTracksAsync(string artistId, string artistName, int take = 25)
    {
        var settings = await _serverSettingsAccessor.GetAsync();
        var tracks = new List<JsonTopTrack>();

        // Try ListenBrainz first (primary source)
        if (settings.ListenBrainzTopTracksEnabled)
        {
            try
            {
                _logger.LogInformation("[TopTracksServiceManager] Attempting ListenBrainz for artist '{ArtistName}'", artistName);
                var lbTracks = await _listenBrainzImporter.GetTopTracksAsync(artistId, take);
                if (lbTracks.Count > 0)
                {
                    tracks.AddRange(lbTracks);
                    _logger.LogInformation("[TopTracksServiceManager] Got {Count} tracks from ListenBrainz for artist '{ArtistName}'", lbTracks.Count, artistName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[TopTracksServiceManager] ListenBrainz failed for artist '{ArtistName}'", artistName);
            }
        }

        // If we don't have enough tracks, try Spotify
        if (tracks.Count < take && settings.SpotifyTopTracksEnabled)
        {
            try
            {
                _logger.LogInformation("[TopTracksServiceManager] Attempting Spotify for artist '{ArtistName}'", artistName);
                var spTracks = await _spotifyImporter.GetTopTracksAsync(artistId, take - tracks.Count);
                if (spTracks.Count > 0)
                {
                    tracks.AddRange(spTracks);
                    _logger.LogInformation("[TopTracksServiceManager] Got {Count} tracks from Spotify for artist '{ArtistName}'", spTracks.Count, artistName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[TopTracksServiceManager] Spotify failed for artist '{ArtistName}'", artistName);
            }
        }

        // If we still don't have enough tracks, try Last.fm
        if (tracks.Count < take && settings.LastFmTopTracksEnabled)
        {
            try
            {
                _logger.LogInformation("[TopTracksServiceManager] Attempting Last.fm for artist '{ArtistName}'", artistName);
                var lfTracks = await _lastFmImporter.GetTopTracksAsync(artistId, take - tracks.Count);
                if (lfTracks.Count > 0)
                {
                    tracks.AddRange(lfTracks);
                    _logger.LogInformation("[TopTracksServiceManager] Got {Count} tracks from Last.fm for artist '{ArtistName}'", lfTracks.Count, artistName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[TopTracksServiceManager] Last.fm failed for artist '{ArtistName}'", artistName);
            }
        }

        // Remove duplicates and take only the requested number
        var uniqueTracks = tracks
            .GroupBy(t => t.Title, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(t => t.PlayCount ?? 0).First())
            .OrderByDescending(t => t.PlayCount ?? 0)
            .Take(take)
            .ToList();

        _logger.LogInformation("[TopTracksServiceManager] Final result: {Count} unique tracks for artist '{ArtistName}'", uniqueTracks.Count, artistName);
        return uniqueTracks;
    }
}
