namespace MusicGQL.Db.Postgres.Models.Events.Folders;

public class CreatedFolder : Event
{
    public Guid FolderId { get; set; }
}
