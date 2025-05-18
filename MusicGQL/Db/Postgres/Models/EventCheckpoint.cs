namespace MusicGQL.Db.Postgres.Models;

public class EventCheckpoint
{
    public string Id { get; set; }
    public int LastProcessedEventId { get; set; }
}
