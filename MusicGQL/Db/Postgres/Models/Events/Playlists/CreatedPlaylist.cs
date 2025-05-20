namespace MusicGQL.Db.Postgres.Models.Events.Playlists;

public class CreatedPlaylist : Event
{
    public Guid PlaylistId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SpotifyPlaylistId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
}
