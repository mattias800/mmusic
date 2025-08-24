using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.Import.Services.TopTracks;

public interface ITopTracksImporter
{
    Task<List<JsonTopTrack>> GetTopTracksAsync(string artistExternalId, int take = 10);
}
