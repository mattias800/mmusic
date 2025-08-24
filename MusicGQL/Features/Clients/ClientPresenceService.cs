using System.Collections.Concurrent;
using System.Security.Claims;
using MusicGQL.Features.Artists;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Clients;

public record ClientPlaybackState(
    string? ArtistId,
    string? ReleaseFolderName,
    int? TrackNumber,
    string? TrackTitle,
    string? ArtistName,
    string? CoverArtUrl,
    int? TrackLengthMs,
    string? QualityLabel
)
{
    public async Task<Artist?> Artist([Service] ServerLibraryCache cache)
    {
        if (string.IsNullOrWhiteSpace(ArtistId))
            return null;
        var a = await cache.GetArtistByIdAsync(ArtistId);
        return a is null ? null : new Artist(a);
    }

    public async Task<Release?> Release([Service] ServerLibraryCache cache)
    {
        if (string.IsNullOrWhiteSpace(ArtistId) || string.IsNullOrWhiteSpace(ReleaseFolderName))
            return null;
        var r = await cache.GetReleaseByArtistAndFolderAsync(ArtistId!, ReleaseFolderName!);
        return r is null ? null : new Release(r);
    }

    public async Task<Track?> Track([Service] ServerLibraryCache cache)
    {
        if (
            string.IsNullOrWhiteSpace(ArtistId)
            || string.IsNullOrWhiteSpace(ReleaseFolderName)
            || TrackNumber is null
        )
            return null;
        var t = await cache.GetTrackByArtistReleaseAndNumberAsync(
            ArtistId!,
            ReleaseFolderName!,
            TrackNumber.Value
        );
        return t is null ? null : new Track(t);
    }
}

public class OnlineClient
{
    public required Guid UserId { get; init; }
    public required string ClientId { get; init; }
    public string Name { get; set; } = "Unnamed client";
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public ClientPlaybackState? Playback { get; set; }
}

public class ClientPresenceService
{
    private readonly ConcurrentDictionary<(Guid userId, string clientId), OnlineClient> _clients =
        new();
    private readonly TimeSpan _offlineAfter = TimeSpan.FromSeconds(45);

    public OnlineClient Heartbeat(
        Guid userId,
        string clientId,
        string? name,
        ClientPlaybackState? playback
    )
    {
        var client = _clients.AddOrUpdate(
            (userId, clientId),
            _ => new OnlineClient
            {
                UserId = userId,
                ClientId = clientId,
                Name = string.IsNullOrWhiteSpace(name) ? "Unnamed client" : name!,
                LastSeenAt = DateTimeOffset.UtcNow,
                Playback = playback,
            },
            (_, existing) =>
            {
                existing.LastSeenAt = DateTimeOffset.UtcNow;
                if (!string.IsNullOrWhiteSpace(name))
                    existing.Name = name!;
                if (playback is not null)
                    existing.Playback = playback;
                return existing;
            }
        );

        return client;
    }

    public IReadOnlyList<OnlineClient> GetOnlineClientsForUser(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        return _clients
            .Where(kv => kv.Key.userId == userId)
            .Select(kv => kv.Value)
            .Where(c => now - c.LastSeenAt <= _offlineAfter)
            .OrderByDescending(c => c.LastSeenAt)
            .ToList();
    }

    public IReadOnlyList<OnlineClient> GetAllOnlineClients()
    {
        var now = DateTimeOffset.UtcNow;
        return _clients
            .Values.Where(c => now - c.LastSeenAt <= _offlineAfter)
            .OrderBy(c => c.UserId)
            .ThenByDescending(c => c.LastSeenAt)
            .ToList();
    }
}
