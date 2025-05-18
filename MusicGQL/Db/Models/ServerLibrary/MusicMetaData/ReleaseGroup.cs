using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class ReleaseGroup
{
    [Key]
    public string Id { get; set; }
    public string Title { get; set; }
    public string FirstReleaseDate { get; set; }
    public string PrimaryType { get; set; }
    public List<string> SecondaryTypes { get; set; }
    public Rating Rating { get; set; }
    public List<NameCredit> Credits { get; set; }
    public List<Release> Releases { get; set; }
    public List<Tag> Tags { get; set; }
    public List<Genre> Genres { get; set; }
    public List<Relation> Relations { get; set; }
    public List<Alias> Aliases { get; set; }
}
