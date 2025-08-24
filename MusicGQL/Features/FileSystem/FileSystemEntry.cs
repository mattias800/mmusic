namespace MusicGQL.Features.FileSystem;

public record FileSystemEntry(
    string Name,
    string Path,
    bool IsDirectory,
    bool HasChildren,
    bool IsAccessible
)
{
    [ID]
    public string Id() => Path;

    public async Task<bool> HasLibraryManifest(
        ServerSettings.LibraryManifestService manifestService
    )
    {
        try
        {
            return await manifestService.HasManifestAsync(Path);
        }
        catch
        {
            return false;
        }
    }
}
