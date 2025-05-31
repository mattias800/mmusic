namespace MusicGQL.Features.Playlists.Db;

public class DbPlaylistItem
{
    public int Id { get; set; }

    public Guid PlaylistId { get; set; }
    public DbPlaylist Playlist { get; set; } = null!;

    public required string RecordingId { get; set; }
    public DateTime AddedAt { get; set; }
}
