using MusicGQL.Features.ServerLibrary.Reader;

namespace MusicGQL.Features.ServerLibrary;

[ExtendObjectType(typeof(MusicGQL.Types.Mutation))]
public class ServerLibraryMaintenanceMutation
{
    public async Task<ScanLibraryResult> ScanLibraryForMissingJson(
        [Service] ServerLibraryFileSystemScanner scanner
    )
    {
        var scan = await scanner.ScanAndFixMissingMetadataAsync();
        return new ScanLibraryResult
        {
            Success = scan.Success,
            ArtistsCreated = scan.ArtistsCreated,
            ReleasesCreated = scan.ReleasesCreated,
            Notes = scan.Notes,
            ErrorMessage = scan.ErrorMessage,
        };
    }
}

public class ScanLibraryResult
{
    public bool Success { get; set; }
    public int ArtistsCreated { get; set; }
    public int ReleasesCreated { get; set; }
    public List<string> Notes { get; set; } = [];
    public string? ErrorMessage { get; set; }
}
