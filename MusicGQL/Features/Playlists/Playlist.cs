using MusicGQL.Features.ServerLibrary.Recording;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.Playlists;

public record Playlist([property: GraphQLIgnore] Db.DbPlaylist Model)
{
    [ID]
    public string Id() => Model.Id.ToString();

    public string? Name() => Model.Name;

    public async Task<IEnumerable<Recording>> Recordings(ServerLibraryService service) =>
        (
            await service.GetRecordingsByIdsAsync(Model.Items.Select(item => item.RecordingId))
        ).Select(r => new Recording(r));

    public DateTime CreatedAt() => Model.CreatedAt;

    public DateTime? ModifiedAt() => Model.ModifiedAt;
}
