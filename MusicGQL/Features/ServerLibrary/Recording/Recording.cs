using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.ServerLibrary.Common;
using MusicGQL.Features.ServerLibrary.Recording.Db;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Recording;

public record Recording([property: GraphQLIgnore] DbRecording Model)
{
    [ID]
    public string Id() => Model.Id;

    public string Title() => Model.Title;

    public int? Length() => Model.Length;

    public async Task<IEnumerable<NameCredit>> NameCredits(ServerLibraryImportService service)
    {
        var credits = await service.GetCreditsOnRecordingAsync(Model.Id);
        return credits.Select(c => new NameCredit(c));
    }

    public async Task<ReleaseGroup.ReleaseGroup?> MainAlbum(ServerLibraryImportService service)
    {
        var releaseGroups = await service.GetReleaseGroupsForRecordingAsync(Model.Id);
        var mainAlbum = LibraryDecider.FindMainReleaseGroupForRecording(releaseGroups);
        return mainAlbum is null ? null : new ReleaseGroup.ReleaseGroup(mainAlbum);
    }

    public async Task<LastFmStatistics?> Statistics(
        LastfmClient lastfmClient,
        ServerLibraryImportService service
    )
    {
        var artists = await service.GetArtistsForRecordingAsync(Model.Id);

        try
        {
            var track = await lastfmClient.Track.GetInfoAsync(Model.Title, artists.First().Name);
            return track is null ? null : new(track.Statistics);
        }
        catch
        {
            return null;
        }
    }

    public async Task<RecordingStreamingServiceInfo> StreamingServiceInfo(
        ServerLibraryImportService service
    )
    {
        var relations = await service.GetRelationsOnRecordingAsync(Model.Id);
        var credits = await service.GetCreditsOnRecordingAsync(Model.Id);
        return new(Model, relations, credits.Select(c => c.NameCredit).ToList());
    }
}
