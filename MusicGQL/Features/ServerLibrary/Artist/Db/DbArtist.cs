namespace MusicGQL.Features.ServerLibrary.Artist.Db;

public class DbArtist
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SortName { get; set; } = string.Empty;
    public string? Gender { get; set; }
}
