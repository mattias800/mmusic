using Microsoft.EntityFrameworkCore;
using MusicGQL.Db.Postgres;
using MusicGQL.Features.Playlists.Events;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Playlists;

public record PlaylistItem([property: GraphQLIgnore] Db.DbPlaylistItem Model)
{
    public int Id() => Model.Id;

    public string RecordingId() => Model.RecordingId;

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

    public bool IsLocalReferencePresent() =>
        !string.IsNullOrWhiteSpace(Model.LocalArtistId)
        && !string.IsNullOrWhiteSpace(Model.LocalReleaseFolderName)
        && Model.LocalTrackNumber.HasValue;

    public async Task<bool> IsLocalAvailable([Service] ServerLibraryCache cache)
    {
        if (IsLocalReferencePresent())
        {
            var track = await cache.GetTrackByArtistReleaseAndNumberAsync(
                Model.LocalArtistId!,
                Model.LocalReleaseFolderName!,
                Model.LocalTrackNumber!.Value
            );
            return track is not null;
        }

        // Fallback: try to parse RecordingId as local ID format artistId/releaseFolder/trackNumber
        var parts = Model.RecordingId.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        if (parts.Length == 3 && int.TryParse(parts[2], out var trackNumber))
        {
            var track = await cache.GetTrackByArtistReleaseAndNumberAsync(
                parts[0],
                parts[1],
                trackNumber
            );
            return track is not null;
        }

        return false;
    }

    public async Task<Track?> Track([Service] ServerLibraryCache cache)
    {
        // Prefer explicit local reference
        if (IsLocalReferencePresent())
        {
            var cached = await cache.GetTrackByArtistReleaseAndNumberAsync(
                Model.LocalArtistId!,
                Model.LocalReleaseFolderName!,
                Model.LocalTrackNumber!.Value
            );
            return cached is null ? null : new Track(cached);
        }

        // Fallback: parse RecordingId
        var parts = Model.RecordingId.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        if (parts.Length == 3 && int.TryParse(parts[2], out var trackNumber))
        {
            var cached = await cache.GetTrackByArtistReleaseAndNumberAsync(
                parts[0],
                parts[1],
                trackNumber
            );
            return cached is null ? null : new Track(cached);
        }

        return null;
    }
}
