using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Utils;

namespace MusicGQL.Features.ServerLibrary;

public record Track([property: GraphQLIgnore] CachedTrack Model)
{
    [ID]
    public string Id() => Model.ArtistId + "/" + Model.ReleaseFolderName + "/" + Model.Title;

    public string Title() => Model.Title;

    public int? Length() => Model.JsonTrack.TrackLength;

    /// <summary>
    /// Gets the audio URL that the server can serve
    /// </summary>
    public string AudioUrl() =>
        LibraryAssetUrlFactory.CreateTrackAudioUrl(
            Model.ArtistId,
            Model.ReleaseFolderName,
            Model.TrackNumber
        );

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
