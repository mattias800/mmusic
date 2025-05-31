using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicGQL.Features.Playlists.Db;

public class DbPlaylistItem
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Playlist")]
    public Guid PlaylistId { get; set; }
    public DbPlaylist Playlist { get; set; } = null!;

    public required string RecordingId { get; set; }
    public DateTime AddedAt { get; set; }
}
