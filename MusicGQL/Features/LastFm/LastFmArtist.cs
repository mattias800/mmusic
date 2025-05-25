using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.ServerLibrary.Artist;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;
using TrackSeries.FanArtTV.Client;

namespace MusicGQL.Features.LastFm;

public record LastFmArtist([property: GraphQLIgnore] Hqub.Lastfm.Entities.Artist Model)
{
    [ID]
    public string Id => Model.Url;

    public string Name => Model.Name;
    public LastFmStatistics Statistics => new(Model.Statistics);
    public string? Summary => Model.Biography.Summary;

    public async Task<Artist?> Artist(ServerLibraryService service)
    {
        var a = await service.SearchArtistByNameAsync(Model.Name);
        var id = a.FirstOrDefault()?.Id;
        if (id is null)
        {
            return null;
        }

        var release = await service.GetArtistByIdAsync(id);
        return release is null ? null : new(release);
    }

    public async Task<ArtistImages?> Images(
        IFanArtTVClient fanartClient,
        MusicBrainzService service
    )
    {
        try
        {
            var a = await service.SearchArtistByNameAsync(Model.Name);
            var id = a.FirstOrDefault()?.Id;
            if (id is null)
            {
                return null;
            }

            var artist = await fanartClient.Music.GetArtistAsync(id);
            return artist is null ? null : new(artist);
        }
        catch
        {
            return null;
        }
    }

    public async Task<MbArtist?> MusicBrainzArtist(MusicBrainzService service)
    {
        var artists = await service.SearchArtistByNameAsync(Model.Name);
        var a = artists.FirstOrDefault();
        return a is null ? null : new MbArtist(a);
    }
}
