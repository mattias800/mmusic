namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;

public class DbReleaseGroup
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PrimaryType { get; set; } = string.Empty;
    public List<string> SecondaryTypes { get; set; } = new();
    public string FirstReleaseDate { get; set; } = string.Empty;
}
