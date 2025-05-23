using MusicGQL.Features.MusicBrainz.Artist;
using MbNameCredit = Hqub.MusicBrainz.Entities.NameCredit;

namespace MusicGQL.Features.MusicBrainz.Common;

public record NameCredit([property: GraphQLIgnore] MbNameCredit Model)
{
    public string Name => Model.Name;
    public MbArtist MbArtist => new(Model.Artist);
    public string? JoinPhrase => Model.JoinPhrase;
}
