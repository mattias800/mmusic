using Hqub.Lastfm;
using MusicGQL.Features.LastFm;
using MusicGQL.Features.ServerLibrary.Json;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary;

public record Track([property: GraphQLIgnore] TrackJson Model)
{
    [ID]
    public string Id() => "Model.Id";

    public string Title() => Model.Title;

    public int? Length() => Model.TrackLength;

    public async Task<LastFmStatistics?> Statistics(
        LastfmClient lastfmClient,
        ServerLibraryService service
    )
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
