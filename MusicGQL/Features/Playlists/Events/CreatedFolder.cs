using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.Features.Playlists.Events;

public class CreatedFolder : Event
{
    public Guid FolderId { get; set; }
}
