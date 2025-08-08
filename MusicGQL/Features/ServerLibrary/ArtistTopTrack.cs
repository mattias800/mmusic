using MusicGQL.Features.ServerLibrary.Cache;
using MusicGQL.Features.ServerLibrary.Json;

namespace MusicGQL.Features.ServerLibrary;

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
        if (!string.IsNullOrWhiteSpace(Model.ReleaseFolderName) && !string.IsNullOrWhiteSpace(Model.CoverArt))
        {
            // Build server URL for cover art based on stored release folder
            var escapedArtistId = Uri.EscapeDataString(ArtistId);
            var escapedReleaseFolderName = Uri.EscapeDataString(Model.ReleaseFolderName);
            return $"/library/{escapedArtistId}/releases/{escapedReleaseFolderName}/coverart";
        }

        // If there is a locally stored top track image (e.g. ./toptrack01.jpg), serve it via dedicated endpoint
        if (!string.IsNullOrWhiteSpace(Model.CoverArt))
        {
            var escapedArtistId = Uri.EscapeDataString(ArtistId);
            return $"/library/{escapedArtistId}/toptracks/{Index}/coverart";
        }

        return null;
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
