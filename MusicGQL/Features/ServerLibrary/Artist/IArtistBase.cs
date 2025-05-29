using Hqub.Lastfm;
using MusicGQL.Features.ServerLibrary.ArtistServerStatus;
using MusicGQL.Features.ServerLibrary.ArtistServerStatus.Services;
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
    Task<ArtistServerStatusResult> ServerStatus(ArtistServerStatusService service);
}
