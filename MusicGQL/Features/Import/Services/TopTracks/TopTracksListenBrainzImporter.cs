using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.ListenBrainz;
using MetaBrainz.ListenBrainz;
using Microsoft.Extensions.Logging;
using System.Net.Http;

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
            
            logger.LogInformation("[TopTracksListenBrainzImporter] ListenBrainz popularity client returned {Count} top recordings for MB artist {MbArtistId}", 
                recordings.Count, mbArtistId);
            
            if (recordings.Count == 0)
            {
                logger.LogWarning("[TopTracksListenBrainzImporter] No top recordings found for MB artist {MbArtistId}. This could mean:", mbArtistId);
                logger.LogWarning("[TopTracksListenBrainzImporter] - The artist MBID doesn't exist in ListenBrainz");
                logger.LogWarning("[TopTracksListenBrainzImporter] - The artist has no listens recorded");
                logger.LogWarning("[TopTracksListenBrainzImporter] - There was an API error");
                return [];
            }

            logger.LogInformation("[TopTracksListenBrainzImporter] Converting {Count} top recordings to JsonTopTrack format for MB artist {MbArtistId}", 
                recordings.Count, mbArtistId);
            
            var topTracks = new List<JsonTopTrack>();
            
            // Use HttpClient for downloading cover art
            using var httpClient = new HttpClient();
            
            // Create all tracks first
            for (int i = 0; i < Math.Min(recordings.Count, take); i++)
            {
                var recording = recordings[i];
                var track = new JsonTopTrack
                {
                    Title = recording.RecordingName,
                    ReleaseTitle = recording.ReleaseName,
                    CoverArt = null, // Will be set below if we can fetch cover art
                    PlayCount = recording.TotalListenCount,
                    TrackLength = recording.Length,
                    RankSource = "listenbrainz", // Mark this as ListenBrainz data
                };
                
                topTracks.Add(track);
            }
            
            // Fetch all cover art in parallel
            var coverArtTasks = new List<Task>();
            
            for (int i = 0; i < topTracks.Count; i++)
            {
                var track = topTracks[i];
                var recording = recordings[i];
                
                if (!string.IsNullOrEmpty(recording.CaaReleaseMbid))
                {
                    var coverArtTask = FetchCoverArtAsync(httpClient, track, recording, i + 1);
                    coverArtTasks.Add(coverArtTask);
                }
                else
                {
                    logger.LogInformation("[TopTracksListenBrainzImporter] No release MBID available for '{Title}', will try Spotify fallback", track.Title);
                }
            }
            
            // Wait for all cover art downloads to complete
            if (coverArtTasks.Count > 0)
            {
                logger.LogInformation("[TopTracksListenBrainzImporter] Starting parallel download of {Count} cover art images", coverArtTasks.Count);
                await Task.WhenAll(coverArtTasks);
                logger.LogInformation("[TopTracksListenBrainzImporter] Completed all cover art downloads");
            }
            
            // Log final results
            for (int i = 0; i < topTracks.Count; i++)
            {
                var track = topTracks[i];
                logger.LogDebug("[TopTracksListenBrainzImporter] Converted recording {Index}: '{Title}' (PlayCount: {PlayCount}, Length: {Length}s, CoverArt: {CoverArt})", 
                    i + 1, track.Title, track.PlayCount, track.TrackLength, track.CoverArt ?? "null");
            }

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
            return [];
        }
    }
    
    private async Task FetchCoverArtAsync(HttpClient httpClient, JsonTopTrack track, ListenBrainzTopRecording recording, int index)
    {
        try
        {
            var coverArtUrl = $"https://coverartarchive.org/release/{recording.CaaReleaseMbid}/front-500";
            logger.LogInformation("[TopTracksListenBrainzImporter] Attempting to fetch cover art for '{Title}' from Cover Art Archive: {Url}", 
                track.Title, coverArtUrl);
            
            var response = await httpClient.GetAsync(coverArtUrl);
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"toptrack{index.ToString("00")}.jpg";
                
                // Note: We can't save the file here because we don't have the artist directory
                // The file will be saved later by the TopTracksCompleter
                // For now, we'll set the CoverArt field to indicate we have a URL to fetch from
                track.CoverArt = coverArtUrl; // This will be processed by the completer
                
                logger.LogInformation("[TopTracksListenBrainzImporter] Successfully fetched cover art for '{Title}' from Cover Art Archive ({Size} bytes)", 
                    track.Title, bytes.Length);
            }
            else
            {
                logger.LogInformation("[TopTracksListenBrainzImporter] Cover Art Archive returned {StatusCode} for '{Title}', will try Spotify fallback", 
                    response.StatusCode, track.Title);
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation("[TopTracksListenBrainzImporter] Failed to fetch cover art for '{Title}' from Cover Art Archive: {Error}, will try Spotify fallback", 
                track.Title, ex.Message);
        }
    }
}
