using Hqub.MusicBrainz.Entities;

namespace MusicGQL.Features.Recording;

public record Relation([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Relation Model)
{
    public string? Type => Model.Type;
    public string? TargetType => Model.TargetType;
    public string? TypeId => Model.TypeId;
    public string? Direction => Model.Direction;
    public string? Begin => Model.Begin;
    public string? End => Model.End;
    public IEnumerable<string>? Attributes => Model.Attributes;
    public Url? Url => Model.Url;
}
