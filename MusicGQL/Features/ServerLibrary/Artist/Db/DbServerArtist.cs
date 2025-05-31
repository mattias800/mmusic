using MusicGQL.Features.Users.Db;

namespace MusicGQL.Features.ServerLibrary.Artist.Db;

public class DbServerArtist
{
    public int Id { get; set; }

    public Guid AddedByUserId { get; set; }
    public DbUser AddedBy { get; set; } = null!;

    public required string ArtistId { get; set; }
    public DateTime AddedAt { get; set; }
}
