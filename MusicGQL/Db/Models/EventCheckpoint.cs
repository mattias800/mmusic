namespace MusicGQL.Db.Models.Projections;

public class EventCheckpoint
{
    public string Id { get; set; }
    public int LastProcessedEventId { get; set; }
}