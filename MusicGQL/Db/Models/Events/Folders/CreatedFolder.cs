namespace MusicGQL.Db.Models.Events.Folders;

public class CreatedFolder: Event
{
    public Guid FolderId { get; set; }
}