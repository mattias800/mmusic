namespace MusicGQL.Db.Postgres.Models;

public abstract class Event
{
    public int Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
