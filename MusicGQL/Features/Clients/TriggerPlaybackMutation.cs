using HotChocolate.Subscriptions;
using MusicGQL.Types;

namespace MusicGQL.Features.Clients;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public sealed class TriggerPlaybackMutation
{
    public async Task<TriggerPlaybackPayload> TriggerPlayback(
        TriggerPlaybackInput input,
        [Service] ClientPresenceService presence,
        [Service] ITopicEventSender sender
    )
    {
        var online = presence.GetAllOnlineClients().Any(c => c.ClientId == input.ClientId);
        await sender.SendAsync("PlaybackTriggered", new ClientPlaybackCommand(
            input.ClientId,
            new ClientPlaybackState(
                input.ArtistId,
                input.ReleaseFolderName,
                input.TrackNumber,
                input.TrackTitle,
                input.ArtistName,
                input.CoverArtUrl,
                input.TrackLengthMs,
                input.QualityLabel
            )
        ));
        return new TriggerPlaybackPayload(input.ClientId, online, online ? null : "Client offline");
    }
}

public record TriggerPlaybackInput(
    string ClientId,
    string ArtistId,
    string ReleaseFolderName,
    int TrackNumber,
    string? TrackTitle,
    string? ArtistName,
    string? CoverArtUrl,
    int? TrackLengthMs,
    string? QualityLabel
);

public record TriggerPlaybackPayload(string ClientId, bool Accepted, string? Message);

public record ClientPlaybackCommand(
    string ClientId,
    ClientPlaybackState Playback
);

[ExtendObjectType(typeof(MusicGQL.Types.Subscription))]
public sealed class ClientPlaybackSubscription
{
    [Subscribe]
    [Topic("PlaybackTriggered")]
    public ClientPlaybackCommand OnPlaybackTriggered([EventMessage] ClientPlaybackCommand command) => command;
}


