namespace MusicGQL.Features.Release;

public record ReleaseGroup([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.ReleaseGroup Model)
{
    [ID] public string Id => Model.Id;
    public string Title => Model.Title;
    public string? PrimaryType => Model.PrimaryType;
    public IEnumerable<string> SecondaryTypes => Model.SecondaryTypes;
    public string? FirstReleaseDate => Model.FirstReleaseDate;
}