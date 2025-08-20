using System.ComponentModel;
using ModelContextProtocol.Server;
using MusicGQL.Features.Clients;
using MusicGQL.Features.Downloads.Services;
using MusicGQL.Features.Likes.Commands;
using MusicGQL.Features.Playlists.Commands;
using MusicGQL.Features.ServerLibrary.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace MusicGQL.Features.McpServer;

[McpServerToolType]
public static class McpTools
{
    [McpServerTool, Description("Search artists, releases, and tracks by name/title.")]
    public static async Task<SearchResults> Search(string query, int limit = 25)
    {
        var cache = McpServiceAccessor.Services.GetRequiredService<ServerLibraryCache>();
        var artists = await cache.SearchArtistsByNameAsync(query, limit);
        var releases = await cache.SearchReleasesByTitleAsync(query, limit);
        var tracks = await cache.SearchTracksByTitleAsync(query, limit);

        return new SearchResults
        {
            Artists = artists.Select(a => new McpArtist
            {
                Id = a.Id,
                Name = a.Name,
                SortName = a.SortName
            }).ToList(),
            Releases = releases.Select(r => new McpRelease
            {
                ArtistId = r.ArtistId,
                ArtistName = r.ArtistName,
                FolderName = r.FolderName,
                Title = r.Title,
                Type = r.Type.ToString()
            }).ToList(),
            Tracks = tracks.Select(t => new McpTrack
            {
                ArtistId = t.ArtistId,
                ArtistName = t.ArtistName,
                ReleaseFolderName = t.ReleaseFolderName,
                ReleaseTitle = t.ReleaseTitle,
                TrackNumber = t.TrackNumber,
                Title = t.Title
            }).ToList()
        };
    }

    [McpServerTool, Description("List all artists in the server library cache.")]
    public static async Task<List<McpArtist>> GetAllArtists()
    {
        var cache = McpServiceAccessor.Services.GetRequiredService<ServerLibraryCache>();
        var artists = await cache.GetAllArtistsAsync();
        return artists.Select(a => new McpArtist { Id = a.Id, Name = a.Name, SortName = a.SortName }).ToList();
    }

    [McpServerTool, Description("List all releases for a given artist id.")]
    public static async Task<List<McpRelease>> GetArtistReleases(string artistId)
    {
        var cache = McpServiceAccessor.Services.GetRequiredService<ServerLibraryCache>();
        var releases = await cache.GetAllReleasesForArtistAsync(artistId);
        return releases.Select(r => new McpRelease
        {
            ArtistId = r.ArtistId,
            ArtistName = r.ArtistName,
            FolderName = r.FolderName,
            Title = r.Title,
            Type = r.Type.ToString()
        }).ToList();
    }

    [McpServerTool, Description("List all tracks for a given artist id and release folder name.")]
    public static async Task<List<McpTrack>> GetReleaseTracks(string artistId, string releaseFolderName)
    {
        var cache = McpServiceAccessor.Services.GetRequiredService<ServerLibraryCache>();
        var tracks = await cache.GetAllTracksForReleaseAsync(artistId, releaseFolderName);
        return tracks.Select(t => new McpTrack
        {
            ArtistId = t.ArtistId,
            ArtistName = t.ArtistName,
            ReleaseFolderName = t.ReleaseFolderName,
            ReleaseTitle = t.ReleaseTitle,
            TrackNumber = t.TrackNumber,
            Title = t.Title
        }).ToList();
    }

    [McpServerTool, Description("List all connected online clients.")]
    public static IReadOnlyList<McpClient> ListOnlineClients()
    {
        var presence = McpServiceAccessor.Services.GetRequiredService<ClientPresenceService>();
        return presence.GetAllOnlineClients().Select(c => new McpClient
        {
            UserId = c.UserId,
            ClientId = c.ClientId,
            Name = c.Name,
            LastSeenAt = c.LastSeenAt,
            Playback = c.Playback == null ? null : new McpPlaybackState
            {
                ArtistId = c.Playback.ArtistId,
                ReleaseFolderName = c.Playback.ReleaseFolderName,
                TrackNumber = c.Playback.TrackNumber,
                TrackTitle = c.Playback.TrackTitle,
                ArtistName = c.Playback.ArtistName,
                CoverArtUrl = c.Playback.CoverArtUrl,
                TrackLengthMs = c.Playback.TrackLengthMs,
                QualityLabel = c.Playback.QualityLabel
            }
        }).ToList();
    }

    [McpServerTool, Description("Trigger playback of a track on a client.")]
    public static async Task<TriggerPlaybackResultDto> TriggerPlayback(
        string clientId,
        string artistId,
        string releaseFolderName,
        int trackNumber,
        string? trackTitle = null,
        string? artistName = null,
        string? coverArtUrl = null,
        int? trackLengthMs = null,
        string? qualityLabel = null
    )
    {
        var presence = McpServiceAccessor.Services.GetRequiredService<ClientPresenceService>();
        var sender = McpServiceAccessor.Services.GetRequiredService<HotChocolate.Subscriptions.ITopicEventSender>();
        var online = presence.GetAllOnlineClients().Any(c => c.ClientId == clientId);
        await sender.SendAsync("PlaybackTriggered", new ClientPlaybackCommand(
            clientId,
            new ClientPlaybackState(
                artistId,
                releaseFolderName,
                trackNumber,
                trackTitle,
                artistName,
                coverArtUrl,
                trackLengthMs,
                qualityLabel
            )
        ));
        return new TriggerPlaybackResultDto { ClientId = clientId, Accepted = online, Message = online ? null : "Client offline" };
    }

    [McpServerTool, Description("Start download for a specific release.")]
    public static async Task<SimpleResult> StartDownloadRelease(string artistId, string releaseFolderName)
    {
        var service = McpServiceAccessor.Services.GetRequiredService<StartDownloadReleaseService>();
        var (ok, err) = await service.StartAsync(artistId, releaseFolderName);
        return new SimpleResult { Success = ok, Message = err };
    }

    [McpServerTool, Description("Start bulk downloads for all/albums/eps/singles for an artist.")]
    public static async Task<SimpleResult> EnqueueBulkDownloads(string artistId, string scope = "All")
    {
        var queue = McpServiceAccessor.Services.GetRequiredService<Features.Downloads.Services.DownloadQueueService>();
        var cache = McpServiceAccessor.Services.GetRequiredService<ServerLibraryCache>();
        var artist = await cache.GetArtistByIdAsync(artistId);
        if (artist is null) return new SimpleResult { Success = false, Message = "Artist not found" };

        var releases = artist.Releases;
        if (string.Equals(scope, "Albums", StringComparison.OrdinalIgnoreCase))
            releases = releases.Where(r => r.Type == Features.ServerLibrary.Json.JsonReleaseType.Album).ToList();
        else if (string.Equals(scope, "Eps", StringComparison.OrdinalIgnoreCase))
            releases = releases.Where(r => r.Type == Features.ServerLibrary.Json.JsonReleaseType.Ep).ToList();
        else if (string.Equals(scope, "Singles", StringComparison.OrdinalIgnoreCase))
            releases = releases.Where(r => r.Type == Features.ServerLibrary.Json.JsonReleaseType.Single).ToList();

        int queued = 0;
        foreach (var r in releases)
        {
            try { queue.Enqueue(new Features.Downloads.DownloadQueueItem(artistId, r.FolderName)); queued++; } catch { }
        }
        return new SimpleResult { Success = true, Message = $"Queued {queued} releases" };
    }

    [McpServerTool, Description("Create a new playlist for the given userId with optional name/description.")]
    public static async Task<CreatePlaylistResponse> CreatePlaylist(Guid userId, string? name = null, string? description = null)
    {
        var handler = McpServiceAccessor.Services.GetRequiredService<CreatePlaylistHandler>();
        await handler.Handle(new CreatePlaylistHandler.Command(userId));
        return new CreatePlaylistResponse { Created = true };
    }

    [McpServerTool, Description("Like a track on behalf of a user (by recording id).")]
    public static async Task<SimpleResult> LikeTrack(Guid userId, string recordingId)
    {
        var likeSongHandler = McpServiceAccessor.Services.GetRequiredService<LikeSongHandler>();
        var res = await likeSongHandler.Handle(new LikeSongHandler.Command(userId, recordingId));
        return res switch
        {
            LikeSongHandler.Result.Success => new SimpleResult { Success = true },
            LikeSongHandler.Result.AlreadyLiked => new SimpleResult { Success = true, Message = "Already liked" },
            LikeSongHandler.Result.SongDoesNotExist => new SimpleResult { Success = false, Message = "Song does not exist" },
            _ => new SimpleResult { Success = false, Message = "Unknown result" }
        };
    }

    [McpServerTool, Description("Get the currently playing track on a client, if any.")]
    public static McpPlaybackState? GetCurrentlyPlaying(string clientId)
    {
        var presence = McpServiceAccessor.Services.GetRequiredService<ClientPresenceService>();
        var client = presence.GetAllOnlineClients().FirstOrDefault(c => c.ClientId == clientId);
        if (client?.Playback == null) return null;
        return new McpPlaybackState
        {
            ArtistId = client.Playback.ArtistId,
            ReleaseFolderName = client.Playback.ReleaseFolderName,
            TrackNumber = client.Playback.TrackNumber,
            TrackTitle = client.Playback.TrackTitle,
            ArtistName = client.Playback.ArtistName,
            CoverArtUrl = client.Playback.CoverArtUrl,
            TrackLengthMs = client.Playback.TrackLengthMs,
            QualityLabel = client.Playback.QualityLabel
        };
    }

    // DTOs for MCP shapes
    public class SearchResults
    {
        public List<McpArtist> Artists { get; set; } = new();
        public List<McpRelease> Releases { get; set; } = new();
        public List<McpTrack> Tracks { get; set; } = new();
    }

    public class McpArtist
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? SortName { get; set; }
    }

    public class McpRelease
    {
        public string ArtistId { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Type { get; set; }
    }

    public class McpTrack
    {
        public string ArtistId { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string ReleaseFolderName { get; set; } = string.Empty;
        public string ReleaseTitle { get; set; } = string.Empty;
        public int TrackNumber { get; set; }
        public string? Title { get; set; }
    }

    public class McpClient
    {
        public Guid UserId { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTimeOffset LastSeenAt { get; set; }
        public McpPlaybackState? Playback { get; set; }
    }

    public class McpPlaybackState
    {
        public string? ArtistId { get; set; }
        public string? ReleaseFolderName { get; set; }
        public int? TrackNumber { get; set; }
        public string? TrackTitle { get; set; }
        public string? ArtistName { get; set; }
        public string? CoverArtUrl { get; set; }
        public int? TrackLengthMs { get; set; }
        public string? QualityLabel { get; set; }
    }

    public class TriggerPlaybackResultDto
    {
        public string ClientId { get; set; } = string.Empty;
        public bool Accepted { get; set; }
        public string? Message { get; set; }
    }

    public class SimpleResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class CreatePlaylistResponse
    {
        public bool Created { get; set; }
    }
}


