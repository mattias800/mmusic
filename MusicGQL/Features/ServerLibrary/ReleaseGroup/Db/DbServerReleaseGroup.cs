using MusicGQL.Features.Users.Db;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;

public class DbServerReleaseGroup
{
    public int Id { get; set; }

    public Guid AddedByUserId { get; set; }
    public DbUser AddedBy { get; set; } = null!;

    public required string ReleaseGroupId { get; set; }
    public DateTime AddedAt { get; set; }
}
