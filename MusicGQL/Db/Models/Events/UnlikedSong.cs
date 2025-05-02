namespace MusicGQL.Db.Models.Events;

public class UnlikedSong : Event
{
    public string ReleaseId { get; set; }
}