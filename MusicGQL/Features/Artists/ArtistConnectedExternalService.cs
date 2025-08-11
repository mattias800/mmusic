using MusicGQL.Features.External;

namespace MusicGQL.Features.Artists;

public record ArtistConnectedExternalService([property: GraphQLIgnore] ExternalServiceModel Model, bool IsConnected)
{
    public ExternalService ExternalService() => new(Model);

    public string ArtistPageUrl() => "";
}