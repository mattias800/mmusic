using Hqub.Lastfm;
using TrackSeries.FanArtTV.Client;

namespace MusicGQL.Features.ServerLibrary.Artist;

[InterfaceType("ArtistBase")]
public interface IArtistBase
{
    [ID]
    string Id();

    string Name();
    string SortName();
    Task<ArtistImages?> Images(IFanArtTVClient fanartClient);
    Task<long?> Listeners(LastfmClient lastfmClient);
    ArtistServerStatus.ArtistServerStatus ServerStatus();
}
