using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.ServerLibrary;

public record ArtistTopTrack(
    [property: GraphQLIgnore] string ArtistId,
    [property: GraphQLIgnore] JsonTopTrack Model
)
{
    public string Title() => Model.Title;

    public string? ReleaseTitle() => Model.ReleaseTitle;

    public async Task<Track?> Track(ServerLibraryCache cache)
    {
        if (Model.ReleaseFolderName == null || Model.TrackNumber == null)
        {
            return null;
        }

        var track = await cache.GetTrackByArtistReleaseAndNumberAsync(
            ArtistId,
            Model.ReleaseFolderName,
            Model.TrackNumber.Value
        );

        return track == null ? null : new(track);
    }
}
