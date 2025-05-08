using MusicGQL.Features.External.SoulSeek.Integration;
using MusicGQL.Types;

namespace MusicGQL.Features.External.SoulSeek;

[ExtendObjectType(typeof(Subscription))]
public record SoulSeekSubscription
{
    [Subscribe]
    public SoulSeekStatus SoulSeekStatusUpdated(
        [EventMessage] SoulSeekState state,
        ILogger<SoulSeekSubscription> logger
    )
    {
        logger.LogInformation("Received Soulseek status update: {Status}", state.NetworkState);
        return new SoulSeekStatus(state);
    }
};
