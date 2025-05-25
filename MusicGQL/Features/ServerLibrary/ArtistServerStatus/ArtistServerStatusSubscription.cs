using MusicGQL.Types;

namespace MusicGQL.Features.ServerLibrary.ArtistServerStatus;

[ExtendObjectType(typeof(Subscription))]
public record ArtistServerStatusSubscription
{
    [Subscribe]
    public ArtistServerStatusResult ArtistServerStatusUpdated(
        [EventMessage] ArtistServerStatusResult s
    ) => s;
}
