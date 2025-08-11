using MusicGQL.Features.Artists;
using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary;

public record TrackCredit([property: GraphQLIgnore] JsonTrackCredit Model)
{
    public string ArtistName() => Model.ArtistName;

    public async Task<Artist?> Artist(ServerLibraryCache cache)
    {
        if (Model.ArtistId == null)
        {
            return null;
        }

        var a = await cache.GetArtistByIdAsync(Model.ArtistId);
        return a != null ? new Artist(a) : null;
    }

    public async Task<MbArtist?> MbArtist(MusicBrainzService mbService)
    {
        if (Model.MusicBrainzArtistId == null)
        {
            return null;
        }

        var a = await mbService.GetArtistByIdAsync(Model.MusicBrainzArtistId);
        return a != null ? new MbArtist(a) : null;
    }
}
