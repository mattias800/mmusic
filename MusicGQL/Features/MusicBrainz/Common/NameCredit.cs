using Hqub.MusicBrainz.Entities;
using MusicGQL.Features.MusicBrainz.Artist;

namespace MusicGQL.Features.MusicBrainz.Common;

public record MbNameCredit([property: GraphQLIgnore] NameCredit Model)
{
    public string Name => Model.Name;
    public MbArtist Artist => new(Model.Artist);
    public string? JoinPhrase => Model.JoinPhrase;
}
