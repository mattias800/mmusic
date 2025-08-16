using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.ListenBrainz;
using MetaBrainz.ListenBrainz;
using Microsoft.Extensions.Logging;

namespace MusicGQL.Features.Import.Services.TopTracks;

public class TopTracksListenBrainzImporter(
    ListenBrainzPopularityClient popularityClient,
    ILogger<TopTracksListenBrainzImporter> logger
) : ITopTracksImporter
{
    public async Task<List<JsonTopTrack>> GetTopTracksAsync(string mbArtistId, int take = 10)
    {
        try
        {
            logger.LogInformation("[TopTracksListenBrainzImporter] Starting to get top tracks for MB artist {MbArtistId}. Requested count: {Take}", 
                mbArtistId, take);
            
            var recordings = await popularityClient.GetTopRecordingsForArtistAsync(mbArtistId);
            
            logger.LogInformation("[TopTracksListenBrainzImporter] ListenBrainz popularity client returned {Count} recordings for MB artist {MbArtistId}", 
                recordings.Count, mbArtistId);
            
            if (recordings.Count == 0)
            {
                logger.LogWarning("[TopTracksListenBrainzImporter] No top recordings found for MB artist {MbArtistId}. This could mean:", mbArtistId);
                logger.LogWarning("[TopTracksListenBrainzImporter] - The artist MBID doesn't exist in ListenBrainz");
                logger.LogWarning("[TopTracksListenBrainzImporter] - The artist has no listens recorded");
                logger.LogWarning("[TopTracksListenBrainzImporter] - There was an API error");
                return [];
            }

            logger.LogInformation("[TopTracksListenBrainzImporter] Converting {Count} recordings to JsonTopTrack format for MB artist {MbArtistId}", 
                recordings.Count, mbArtistId);
            
            var topTracks = recordings
                .OrderByDescending(r => r.TotalListenCount)
                .Take(take)
                .Select((recording, index) => 
                {
                    var track = new JsonTopTrack
                    {
                        Title = recording.RecordingName,
                        ReleaseTitle = recording.ReleaseName,
                        CoverArt = null, // Will be set later when matched to local release
                        PlayCount = recording.TotalListenCount,
                        TrackLength = recording.Length,
                        // Additional ListenBrainz-specific data could be stored here if needed
                    };
                    
                    logger.LogDebug("[TopTracksListenBrainzImporter] Converted recording {Index}: '{Title}' (PlayCount: {PlayCount}, Length: {Length}s)", 
                        index + 1, track.Title, track.PlayCount, track.TrackLength);
                    
                    return track;
                })
                .ToList();

            logger.LogInformation("[TopTracksListenBrainzImporter] Successfully converted {Count} recordings to top tracks for MB artist {MbArtistId}. Final track count: {FinalCount}", 
                recordings.Count, mbArtistId, topTracks.Count);
            
            if (topTracks.Count > 0)
            {
                var topTrack = topTracks[0];
                logger.LogInformation("[TopTracksListenBrainzImporter] Top track: '{Title}' with {PlayCount} plays", 
                    topTrack.Title, topTrack.PlayCount);
            }
            
            return topTracks;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[TopTracksListenBrainzImporter] Unexpected error while getting top tracks for MB artist {MbArtistId}", mbArtistId);
            logger.LogError("[TopTracksListenBrainzImporter] Error details: {ErrorMessage}", ex.Message);
            logger.LogError("[TopTracksListenBrainzImporter] Stack trace: {StackTrace}", ex.StackTrace);
            return [];
        }
    }
}
