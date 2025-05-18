using System.ComponentModel.DataAnnotations;

namespace MusicGQL.Db.Models.ServerLibrary.MusicMetaData;

public class Artist
{
    [Key]
    public string Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string SortName { get; set; }
    public string Gender { get; set; }
    public LifeSpan LifeSpan { get; set; }
    public Area? Area { get; set; }
    public Area? BeginArea { get; set; }
    public Area? EndArea { get; set; }
    public string Country { get; set; }
    public string Disambiguation { get; set; }
    public Rating Rating { get; set; }
    public List<Recording> Recordings { get; set; }
    public List<ReleaseGroup> ReleaseGroups { get; set; }
    public List<Release> Releases { get; set; }
    public List<Work> Works { get; set; }
    public List<Tag> Tags { get; set; }
    public List<Genre> Genres { get; set; }
    public List<Relation> Relations { get; set; }
    public List<Alias> Aliases { get; set; }
}
