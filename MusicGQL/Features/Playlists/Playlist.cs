using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Playlists;

public record Playlist([property: GraphQLIgnore] Db.DbPlaylist Model)
{
    [ID]
    public string Id() => Model.Id.ToString();

    public string? Name() => Model.Name;

    public async Task<IEnumerable<Track>> Tracks(ServerLibraryCache cache) => [];

    public DateTime CreatedAt() => Model.CreatedAt;

    public DateTime? ModifiedAt() => Model.ModifiedAt;

    public string? CoverImageUrl() => Model.CoverImageUrl;
}
