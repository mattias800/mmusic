using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.ListenBrainz;
using MetaBrainz.ListenBrainz;
using Microsoft.Extensions.Logging;

namespace MusicGQL.Features.Import.Services.TopTracks;

public class TopTracksListenBrainzImporter(
    ListenBrainzService listenBrainzService,
    ILogger<TopTracksListenBrainzImporter> logger
) : ITopTracksImporter
{
    public async Task<List<JsonTopTrack>> GetTopTracksAsync(string mbArtistId, int take = 10)
    {
        try
        {
            // ListenBrainz doesn't have a direct "top tracks by artist" endpoint
            // We need to get user listens and aggregate them by artist
            // For now, we'll return an empty list as this requires a different approach
            // TODO: Implement by getting user listens and aggregating track play counts
            
            logger.LogInformation("[TopTracksListenBrainzImporter] Getting top tracks for MB artist {MbArtistId}", mbArtistId);
            
            // Note: This would require:
            // 1. Getting user listens for the artist
            // 2. Aggregating track play counts
            // 3. Or using a different ListenBrainz endpoint
            
            return [];
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[TopTracksListenBrainzImporter] Failed to get top tracks for MB artist {MbArtistId}", mbArtistId);
            return [];
        }
    }
}
