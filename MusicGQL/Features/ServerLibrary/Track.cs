using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.ServerLibrary;

public record Track([property: GraphQLIgnore] CachedTrack Model)
{
    [ID]
    public string Id() => Model.ArtistId + "/" + Model.AlbumFolderName + "/" + Model.Title;

    public string Title() => Model.Title;

    public int? Length() => Model.TrackJson.TrackLength;

    public async Task<LastFmStatistics?> Statistics(LastfmClient lastfmClient)
    {
        return null;
        // var artists = await service.GetArtistsForRecordingAsync(Model.Id);
        //
        // try
        // {
        //     var track = await lastfmClient.Track.GetInfoAsync(Model.Title, artists.First().Name);
        //     return track is null ? null : new(track.Statistics);
        // }
        // catch
        // {
        //     return null;
        // }
    }
}
