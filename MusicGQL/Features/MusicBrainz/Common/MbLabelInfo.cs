namespace MusicGQL.Features.MusicBrainz.Common;

public record MbLabelInfo([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.LabelInfo Model)
{
    public string? CatalogNumber() => Model.CatalogNumber;

    public MbLabel Label() => new(Model.Label);
}
