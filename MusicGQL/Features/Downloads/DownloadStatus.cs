using MusicGQL.Sagas.DownloadRelease;

namespace MusicGQL.Features.Downloads;

public record DownloadStatus([property: GraphQLIgnore] DownloadReleaseSagaData Model)
{
    [ID]
    public string GetId() => Model.Id.ToString();

    public Release.Release? Release => Model.Release is null ? null : new Release.Release(Model.Release);
}