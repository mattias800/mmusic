namespace MusicGQL.Db.Models.Events.Folders;

public class RenamedFolder : Event
{
    public Guid FolderId { get; set; }
    public string NewFolderName { get; set; }
}