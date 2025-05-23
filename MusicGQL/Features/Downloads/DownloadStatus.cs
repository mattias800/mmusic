using MusicGQL.Features.Downloads.Sagas;
using MusicGQL.Features.MusicBrainz.Release;

namespace MusicGQL.Features.Downloads;

public record DownloadStatus([property: GraphQLIgnore] DownloadReleaseSagaData Model)
{
    [ID]
    public string GetId() => Model.Id.ToString();

    public string? ArtistName() => Model.ArtistName;

    public string? ReleaseName() => Model.ReleaseName;

    public string StatusDescription() => Model.StatusDescription;

    public int? NumberOfTracks() => Model.NumberOfTracks;

    public int? TracksDownloaded() => Model.TracksDownloaded;

    public MbRelease? Release => Model.Release is null ? null : new MbRelease(Model.Release);
}
