using MusicGQL.Features.Playlists.Events;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Playlists;

public record PlaylistItem([property: GraphQLIgnore] Db.DbPlaylistItem Model)
{
    [ID]
    public string Id() => Model.Id;

    public DateTime AddedAt() => Model.AddedAt;

    public ExternalServiceType? ExternalService() => Model.ExternalService;

    public string? ExternalTrackId() => Model.ExternalTrackId;

    public string? ExternalAlbumId() => Model.ExternalAlbumId;

    public string? ExternalArtistId() => Model.ExternalArtistId;

    public string? Title() => Model.SongTitle;

    public string? ArtistName() => Model.ArtistName;

    public string? ReleaseTitle() => Model.ReleaseTitle;

    public string? ReleaseType() => Model.ReleaseType;

    public int? TrackLengthMs() => Model.TrackLengthMs;

    public string? CoverImageUrl() => Model.LocalCoverImageUrl ?? Model.CoverImageUrl;

    public string? LocalCoverImageUrl() => Model.LocalCoverImageUrl;

    public async Task<Artist?> Artist([Service] ServerLibraryCache cache)
    {
        if (Model.LocalArtistId is null)
        {
            return null;
        }

        var cached = await cache.GetArtistByIdAsync(Model.LocalArtistId);
        return cached is null ? null : new Artist(cached);
    }

    public async Task<Track?> Track([Service] ServerLibraryCache cache)
    {
        if (
            Model is
            {
                LocalArtistId: not null,
                LocalReleaseFolderName: not null,
                LocalTrackNumber: not null
            }
        )
        {
            var cached = await cache.GetTrackByArtistReleaseAndNumberAsync(
                Model.LocalArtistId,
                Model.LocalReleaseFolderName,
                Model.LocalTrackNumber.Value
            );
            return cached is null ? null : new Track(cached);
        }

        return null;
    }
}
