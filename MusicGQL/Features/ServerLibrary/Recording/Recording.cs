using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.ServerLibrary.Recording.Db;

namespace MusicGQL.Features.ServerLibrary.Recording;

public record Recording([property: GraphQLIgnore] DbRecording Model)
{
    [ID]
    public string Id => Model.Id;
    public string Title => Model.Title;
    public int? Length => Model.Length;

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
}
