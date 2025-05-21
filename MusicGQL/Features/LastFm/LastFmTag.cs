namespace MusicGQL.Features.LastFm;

public record LastFmTag([property: GraphQLIgnore] Hqub.Lastfm.Entities.Tag Model)
{
    public string Name => Model.Name;
    public string Url => Model.Url;
}
