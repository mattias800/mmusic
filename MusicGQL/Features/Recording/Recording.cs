namespace MusicGQL.Features.Recording;

public record Recording([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Recording Model)
{
    [ID] public string Id => Model.Id;
    public string Title => Model.Title;
}