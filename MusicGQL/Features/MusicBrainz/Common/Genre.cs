namespace MusicGQL.Features.MusicBrainz.Common;

public record Genre([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Genre Model)
{
    [ID]
    public string Id => Model.Id;
    public string Name => Model.Name;
}
