using MbNameCredit = Hqub.MusicBrainz.Entities.NameCredit;

namespace MusicGQL.Features.Recording;

public record NameCredit(MbNameCredit Model)
{
    public string Name => Model.Name;
    public Artist.Artist Artist => new(Model.Artist);
    public string? JoinPhrase => Model.JoinPhrase;
}