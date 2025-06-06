using Hqub.MusicBrainz.Entities;

namespace MusicGQL.Features.MusicBrainz.Common;

public record MbRelation([property: GraphQLIgnore] Relation Model)
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
