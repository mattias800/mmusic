using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.Artists;

public record ArtistTopTrack(
    [property: GraphQLIgnore] string ArtistId,
    [property: GraphQLIgnore] JsonTopTrack Model,
    [property: GraphQLIgnore] int Index
)
{
    public string Title() => Model.Title;

    public string? ReleaseTitle() => Model.ReleaseTitle;

    public long? PlayCount() => Model.PlayCount;

    public int? TrackLength() => Model.TrackLength;

    public string? CoverArtUrl()
    {
        // Always point to the unified toptracks endpoint; the endpoint will read artist.json
        // and decide whether to serve release cover art or a local toptrackNN.jpg.
        var escapedArtistId = Uri.EscapeDataString(ArtistId);
        return $"/library/{escapedArtistId}/toptracks/{Index}/coverart";
    }

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
