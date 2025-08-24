using Microsoft.Extensions.Logging;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Features.ServerSettings;

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
        ILogger<TopTracksServiceManager> logger
    )
    {
        _listenBrainzImporter = listenBrainzImporter;
        _spotifyImporter = spotifyImporter;
        _lastFmImporter = lastFmImporter;
        _serverSettingsAccessor = serverSettingsAccessor;
        _logger = logger;
    }

    public async Task<TopTracksResult> GetTopTracksAsync(
        string mbArtistId,
        string artistDisplayName,
        int take = 25
    )
    {
        var settings = await _serverSettingsAccessor.GetAsync();
        var result = new TopTracksResult();

        _logger.LogInformation(
            "[TopTracksServiceManager] Getting top tracks for artist '{ArtistName}' (MBID: {MbArtistId})",
            artistDisplayName,
            mbArtistId
        );

        _logger.LogInformation(
            "[TopTracksServiceManager] Service configuration - ListenBrainz: {ListenBrainzEnabled}, Spotify: {SpotifyEnabled}, LastFm: {LastFmEnabled}",
            settings.ListenBrainzTopTracksEnabled,
            settings.SpotifyTopTracksEnabled,
            settings.LastFmTopTracksEnabled
        );

        // Try ListenBrainz first if enabled
        if (settings.ListenBrainzTopTracksEnabled)
        {
            try
            {
                _logger.LogInformation(
                    "[TopTracksServiceManager] ListenBrainz is enabled. Attempting to get top tracks for artist '{ArtistName}'",
                    artistDisplayName
                );
                var listenBrainzTracks = await _listenBrainzImporter.GetTopTracksAsync(
                    mbArtistId,
                    take
                );

                _logger.LogInformation(
                    "[TopTracksServiceManager] ListenBrainz importer returned {Count} tracks for artist '{ArtistName}'",
                    listenBrainzTracks.Count,
                    artistDisplayName
                );

                if (listenBrainzTracks.Count > 0)
                {
                    result.Tracks = listenBrainzTracks;
                    result.Source = "ListenBrainz";
                    result.Success = true;
                    _logger.LogInformation(
                        "[TopTracksServiceManager] ListenBrainz succeeded with {Count} tracks for artist '{ArtistName}'",
                        listenBrainzTracks.Count,
                        artistDisplayName
                    );
                    return result;
                }
                else
                {
                    _logger.LogWarning(
                        "[TopTracksServiceManager] ListenBrainz returned no tracks for artist '{ArtistName}'",
                        artistDisplayName
                    );
                    result.Warnings.Add("ListenBrainz returned no tracks for this artist");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[TopTracksServiceManager] ListenBrainz failed with exception for artist '{ArtistName}': {ErrorMessage}",
                    artistDisplayName,
                    ex.Message
                );
                result.Warnings.Add($"ListenBrainz failed: {ex.Message}");
            }
        }
        else
        {
            _logger.LogInformation(
                "[TopTracksServiceManager] ListenBrainz is disabled for artist '{ArtistName}'",
                artistDisplayName
            );
        }

        // Try Spotify if enabled and ListenBrainz failed
        if (settings.SpotifyTopTracksEnabled)
        {
            try
            {
                _logger.LogInformation(
                    "[TopTracksServiceManager] Spotify is enabled. Attempting to get top tracks for artist '{ArtistName}'",
                    artistDisplayName
                );
                var spotifyTracks = await _spotifyImporter.GetTopTracksAsync(mbArtistId, take);

                _logger.LogInformation(
                    "[TopTracksServiceManager] Spotify importer returned {Count} tracks for artist '{ArtistName}'",
                    spotifyTracks.Count,
                    artistDisplayName
                );

                if (spotifyTracks.Count > 0)
                {
                    result.Tracks = spotifyTracks;
                    result.Source = "Spotify";
                    result.Success = true;
                    _logger.LogInformation(
                        "[TopTracksServiceManager] Spotify succeeded with {Count} tracks for artist '{ArtistName}'",
                        spotifyTracks.Count,
                        artistDisplayName
                    );
                    return result;
                }
                else
                {
                    _logger.LogWarning(
                        "[TopTracksServiceManager] Spotify returned no tracks for artist '{ArtistName}'",
                        artistDisplayName
                    );
                    result.Warnings.Add("Spotify returned no tracks for this artist");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[TopTracksServiceManager] Spotify failed with exception for artist '{ArtistName}': {ErrorMessage}",
                    artistDisplayName,
                    ex.Message
                );
                result.Warnings.Add($"Spotify failed: {ex.Message}");
            }
        }
        else
        {
            _logger.LogInformation(
                "[TopTracksServiceManager] Spotify is disabled for artist '{ArtistName}'",
                artistDisplayName
            );
        }

        // Try Last.fm if enabled and others failed
        if (settings.LastFmTopTracksEnabled)
        {
            try
            {
                _logger.LogInformation(
                    "[TopTracksServiceManager] Last.fm is enabled. Attempting to get top tracks for artist '{ArtistName}'",
                    artistDisplayName
                );
                var lastFmTracks = await _lastFmImporter.GetTopTracksAsync(mbArtistId, take);

                _logger.LogInformation(
                    "[TopTracksServiceManager] Last.fm importer returned {Count} tracks for artist '{ArtistName}'",
                    lastFmTracks.Count,
                    artistDisplayName
                );

                if (lastFmTracks.Count > 0)
                {
                    result.Tracks = lastFmTracks;
                    result.Source = "Last.fm";
                    result.Success = true;
                    _logger.LogInformation(
                        "[TopTracksServiceManager] Last.fm succeeded with {Count} tracks for artist '{ArtistName}'",
                        lastFmTracks.Count,
                        artistDisplayName
                    );
                    return result;
                }
                else
                {
                    _logger.LogWarning(
                        "[TopTracksServiceManager] Last.fm returned no tracks for artist '{ArtistName}'",
                        artistDisplayName
                    );
                    result.Warnings.Add("Last.fm returned no tracks for this artist");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[TopTracksServiceManager] Last.fm failed with exception for artist '{ArtistName}': {ErrorMessage}",
                    artistDisplayName,
                    ex.Message
                );
                result.Warnings.Add($"Last.fm failed: {ex.Message}");
            }
        }
        else
        {
            _logger.LogInformation(
                "[TopTracksServiceManager] Last.fm is disabled for artist '{ArtistName}'",
                artistDisplayName
            );
        }

        // All services failed or returned no tracks
        result.Success = false;
        result.Warnings.Add("All enabled top tracks services failed or returned no data");

        _logger.LogWarning(
            "[TopTracksServiceManager] All top tracks services failed for artist '{ArtistName}'. Final warnings: {Warnings}",
            artistDisplayName,
            string.Join("; ", result.Warnings)
        );

        return result;
    }
}

public class TopTracksResult
{
    public List<JsonTopTrack> Tracks { get; set; } = [];
    public string Source { get; set; } = string.Empty;
    public bool Success { get; set; }
    public List<string> Warnings { get; set; } = [];
}
