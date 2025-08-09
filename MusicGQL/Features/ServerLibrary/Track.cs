using Hqub.Lastfm;
using MusicGQL.Features.Downloads;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Utils;

namespace MusicGQL.Features.ServerLibrary;

public record Track([property: GraphQLIgnore] CachedTrack Model)
{
    [ID]
    public string Id() => Model.ArtistId + "/" + Model.ReleaseFolderName + "/" + Model.Title;

    public string Title() => Model.Title;

    public int TrackNumber() => Model.JsonTrack.TrackNumber;

    public int? TrackLength() => Model.JsonTrack.TrackLength;

    public long? PlayCount() => Model.JsonTrack.PlayCount;

    public bool IsMissing() => string.IsNullOrWhiteSpace(Model.JsonTrack.AudioFilePath);

    /// <summary>
    /// Gets the audio URL that the server can serve
    /// </summary>
    public string AudioUrl() =>
        LibraryAssetUrlFactory.CreateTrackAudioUrl(
            Model.ArtistId,
            Model.ReleaseFolderName,
            Model.TrackNumber
        );

    public async Task<Release> Release(ServerLibraryCache cache)
    {
        var release = await cache.GetReleaseByArtistAndFolderAsync(
            Model.ArtistId,
            Model.ReleaseFolderName
        );

        if (release is null)
        {
            throw new Exception(
                "Could not find release, artistId="
                    + Model.ArtistId
                    + ", folderName="
                    + Model.ReleaseFolderName
            );
        }

        return new(release);
    }

    public MediaAvailabilityStatus MediaAvailabilityStatus() =>
        Model.CachedMediaAvailabilityStatus.ToGql();

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
