using MusicGQL.Integration.Neo4j.Models;

namespace MusicGQL.Features.ServerLibrary.Common;

public record NameCredit([property: GraphQLIgnore] ArtistCredit Model)
{
    public string JoinPhrase => Model.NameCredit.JoinPhrase;
    public string Name => Model.NameCredit.Name;
    public Artist.Artist Artist => new(Model.Artist);
}
