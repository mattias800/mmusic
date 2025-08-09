using MusicGQL.Features.Downloads;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Utils;
using IO = System.IO;

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

    public TrackMedia? Media() =>
        string.IsNullOrWhiteSpace(Model.JsonTrack.AudioFilePath) ? new(Model) : null;

    public MediaAvailabilityStatus MediaAvailabilityStatus() =>
        Model.CachedMediaAvailabilityStatus.ToGql();

    public IEnumerable<TrackCredit> Credits() =>
        Model.JsonTrack.Credits?.Select(t => new TrackCredit(t)) ?? [];

    public TrackStatistics? Statistics()
    {
        return Model.JsonTrack.Statistics is null
            ? null
            : new TrackStatistics(Model.JsonTrack.Statistics);
    }
}
