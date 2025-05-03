namespace MusicGQL.Db.Models;

public class EventCheckpoint
{
    public string Id { get; set; }
    public int LastProcessedEventId { get; set; }
}