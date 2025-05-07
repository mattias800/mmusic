using MbNameCredit = Hqub.MusicBrainz.Entities.NameCredit;

namespace MusicGQL.Features.NameCredit;

public record NameCredit([property: GraphQLIgnore] MbNameCredit Model)
{
    public string Name => Model.Name;
    public Artist.Artist Artist => new(Model.Artist);
    public string? JoinPhrase => Model.JoinPhrase;
}
