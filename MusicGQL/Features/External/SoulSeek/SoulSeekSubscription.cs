using MusicGQL.Types;

namespace MusicGQL.Features.External.SoulSeek;

[ExtendObjectType(typeof(Subscription))]
public abstract record SoulSeekSubscription
{
    [Subscribe]
    public SoulSeekStatus SoulSeekStatusUpdated([EventMessage] string s) => new();
};
