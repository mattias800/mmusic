using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary;

public record TrackCredit([property: GraphQLIgnore] CachedTrackCredit Model)
{
    public string ArtistName() => Model.JsonTrackCredit.ArtistName;

    public async Task<Artist?> ArtistName(ServerLibraryCache cache)
    {
        if (Model.JsonTrackCredit.ArtistId == null)
        {
            return null;
        }

        var a = await cache.GetArtistByIdAsync(Model.JsonTrackCredit.ArtistId);
        return a != null ? new Artist(a) : null;
    }

    public async Task<MbArtist?> ArtistName(ServerLibraryCache cache, MusicBrainzService mbService)
    {
        if (Model.JsonTrackCredit.MusicBrainzArtistId == null)
        {
            return null;
        }

        var a = await mbService.GetArtistByIdAsync(Model.JsonTrackCredit.MusicBrainzArtistId);
        return a != null ? new MbArtist(a) : null;
    }
}
