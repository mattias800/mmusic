using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class SongAddedToPlaylist : Event
{
    public required Guid PlaylistId { get; set; }
    public required string RecordingId { get; set; }

    /**
     * If null, the song was added to the end of the playlist.
     */
    public int? Position { get; set; }

    // Optional reference to a local library track
    public string? LocalArtistId { get; set; }
    public string? LocalReleaseFolderName { get; set; }
    public int? LocalTrackNumber { get; set; }

    // Optional reference to an external service track (e.g., Spotify)
    public ExternalServiceType? ExternalService { get; set; }
    public string? ExternalTrackId { get; set; }
    public string? ExternalAlbumId { get; set; }
    public string? ExternalArtistId { get; set; }

    // Portable metadata to render UI even when local library item is missing
    public string? SongTitle { get; set; }
    public string? ArtistName { get; set; }
    public string? ReleaseTitle { get; set; }
    public string? ReleaseType { get; set; } // Album | Single | EP | Track
    public int? TrackLengthMs { get; set; }

    // Cover art references. LocalCoverImageUrl is preferred if available
    public string? CoverImageUrl { get; set; } // Source/original external image URL
    public string? LocalCoverImageUrl { get; set; } // If downloaded and hosted locally
}
