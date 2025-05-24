using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.ServerLibrary.Artist;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.LastFm;

public record LastFmArtist([property: GraphQLIgnore] Hqub.Lastfm.Entities.Artist Model)
{
    [ID]
    public string Id => Model.MBID;

    public string Name => Model.Name;
    public LastFmStatistics Statistics => new(Model.Statistics);
    public string? Summary => Model.Biography.Summary;

    public async Task<Artist?> Artist(Neo4jService service)
    {
        if (string.IsNullOrEmpty(Model.MBID))
        {
            return null;
        }

        var release = await service.GetArtistByIdAsync(Model.MBID);
        return release is null ? null : new(release);
    }

    public async Task<MbArtist?> MusicBrainzArtist(MusicBrainzService service)
    {
        if (string.IsNullOrEmpty(Model.MBID))
        {
            return null;
        }

        var release = await service.GetArtistByIdAsync(Model.MBID);
        return release is null ? null : new MbArtist(release);
    }
}
