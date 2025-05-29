namespace MusicGQL.Features.ServerLibrary.Release.Db;

public class DbRelease
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Date { get; set; }
    public string? Status { get; set; }
    public string? Barcode { get; set; }
    public string? Country { get; set; }
    public string? Quality { get; set; }
}
