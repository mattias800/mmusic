namespace MusicGQL.Features.Release;

public record Genre([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Genre Model)
{
    [ID] public string Id => Model.Id;
    public string Name => Model.Name;
}