using MusicGQL.Features.External;

namespace MusicGQL.Features.Artists;

public record ConnectedExternalService([property: GraphQLIgnore] ExternalServiceModel Model, bool IsConnected)
{
    public ExternalService ExternalService() => new(Model);
}