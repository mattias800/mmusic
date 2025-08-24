namespace MusicGQL.Features.External;

public record ExternalService([property: GraphQLIgnore] ExternalServiceModel Model)
{
    [ID]
    public string Id() => Model.Id;

    public string Name() => Model.Name;
}
