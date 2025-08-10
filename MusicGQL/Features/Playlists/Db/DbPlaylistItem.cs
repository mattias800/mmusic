namespace MusicGQL.Features.Playlists.Db;

public class DbPlaylistItem
{
    public int Id { get; set; }

    public Guid PlaylistId { get; set; }
    public DbPlaylist Playlist { get; set; } = null!;

    public required string RecordingId { get; set; }
    public DateTime AddedAt { get; set; }

    // Optional local library reference
    public string? LocalArtistId { get; set; }
    public string? LocalReleaseFolderName { get; set; }
    public int? LocalTrackNumber { get; set; }

    // Optional external service references
    public Events.ExternalServiceType? ExternalService { get; set; }
    public string? ExternalTrackId { get; set; }
    public string? ExternalAlbumId { get; set; }
    public string? ExternalArtistId { get; set; }

    // Portable display metadata
    public string? SongTitle { get; set; }
    public string? ArtistName { get; set; }
    public string? ReleaseTitle { get; set; }
    public string? ReleaseType { get; set; }
    public int? TrackLengthMs { get; set; }

    // Cover art URLs
    public string? CoverImageUrl { get; set; }
    public string? LocalCoverImageUrl { get; set; }
}
