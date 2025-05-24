namespace MusicGQL.Features.MusicBrainz.Common;

public record MbLabel([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Label Model)
{
    public string Id() => Model.Id;

    public string Name() => Model.Name;

    public string? Disambiguation() => Model.Disambiguation;
    // public List<Alias> Aliases() => Model.Aliases ;
}
