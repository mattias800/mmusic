using System.Collections.Concurrent;
using System.Security.Claims;

namespace MusicGQL.Features.Clients;

public record ClientPlaybackState(
    string? ArtistId,
    string? ReleaseFolderName,
    int? TrackNumber,
    string? TrackTitle
);

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
    private readonly ConcurrentDictionary<(Guid userId, string clientId), OnlineClient> _clients = new();
    private readonly TimeSpan _offlineAfter = TimeSpan.FromSeconds(45);

    public OnlineClient Heartbeat(Guid userId, string clientId, string? name, ClientPlaybackState? playback)
    {
        var client = _clients.AddOrUpdate(
            (userId, clientId),
            _ => new OnlineClient
            {
                UserId = userId,
                ClientId = clientId,
                Name = string.IsNullOrWhiteSpace(name) ? "Unnamed client" : name!,
                LastSeenAt = DateTimeOffset.UtcNow,
                Playback = playback
            },
            (_, existing) =>
            {
                existing.LastSeenAt = DateTimeOffset.UtcNow;
                if (!string.IsNullOrWhiteSpace(name)) existing.Name = name!;
                if (playback is not null) existing.Playback = playback;
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
        return _clients.Values
            .Where(c => now - c.LastSeenAt <= _offlineAfter)
            .OrderBy(c => c.UserId)
            .ThenByDescending(c => c.LastSeenAt)
            .ToList();
    }
}


