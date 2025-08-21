using HotChocolate;
using HotChocolate.Subscriptions;
using MusicGQL.Types;

namespace MusicGQL.Features.Clients;

[ExtendObjectType(typeof(Subscription))]
public sealed class ClientPresenceSubscription
{
    // Field name will be onClientsUpdated (camelCase of method name)
    [Subscribe]
    [Topic("ClientsUpdated")]
    public IReadOnlyList<OnlineClient> OnClientsUpdated([EventMessage] IReadOnlyList<OnlineClient> clients) => clients;
}


