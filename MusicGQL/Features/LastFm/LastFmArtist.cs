using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.LastFm;

public record LastFmArtist([property: GraphQLIgnore] Hqub.Lastfm.Entities.Artist Model)
{
    [ID]
    public string Id => Model.MBID;

    public string Name => Model.Name;
    public LastFmStatistics Statistics => new(Model.Statistics);
    public string? Summary => Model.Biography.Summary;

    public async Task<Artist.Artist?> Artist([Service] MusicBrainzService mbService)
    {
        if (string.IsNullOrEmpty(Model.MBID))
        {
            return null;
        }
        var release = await mbService.GetArtistByIdAsync(Model.MBID);
        return release is null ? null : new Artist.Artist(release);
    }
}
