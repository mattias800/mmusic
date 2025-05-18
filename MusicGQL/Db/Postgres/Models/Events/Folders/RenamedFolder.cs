namespace MusicGQL.Db.Postgres.Models.Events.Folders;

public class RenamedFolder : Event
{
    public Guid FolderId { get; set; }
    public string NewFolderName { get; set; }
}
