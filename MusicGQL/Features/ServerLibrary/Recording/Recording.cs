using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.MusicBrainz.Recording;
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

    public async Task<LastFmStatistics?> Statistics([Service] LastfmClient lastfmClient)
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

    public RecordingStreamingServiceInfo StreamingServiceInfo() => new(Model);
}
