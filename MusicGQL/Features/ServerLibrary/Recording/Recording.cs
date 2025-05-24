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

    public async Task<IEnumerable<NameCredit>> NameCredits(Neo4jService service)
    {
        var credits = await service.GetCreditsOnRecordingAsync(Model.Id);
        return credits.Select(c => new NameCredit(c));
    }

    public async Task<Release.Release?> MainAlbum(Neo4jService service)
    {
        var releases = await service.GetReleasesForRecordingAsync(Model.Id);
        var mainAlbum = releases.FirstOrDefault(); // We only store main albums in Neo4j
        return mainAlbum is null ? null : new Release.Release(mainAlbum);
    }

    public async Task<LastFmStatistics?> Statistics(LastfmClient lastfmClient)
    {
        try
        {
            var track = await lastfmClient.Track.GetInfoByMbidAsync(Model.Id);
            return track is null ? null : new(track.Statistics);
        }
        catch
        {
            return null;
        }
    }

    public async Task<RecordingStreamingServiceInfo> StreamingServiceInfo(Neo4jService service)
    {
        var relations = await service.GetRelationsOnRecordingAsync(Model.Id);
        var credits = await service.GetCreditsOnRecordingAsync(Model.Id);
        return new(Model, relations, credits.Select(c => c.NameCredit).ToList());
    }
}
