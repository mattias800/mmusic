using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class RenamedFolder : Event
{
    public Guid FolderId { get; set; }
    public string NewFolderName { get; set; }
}
