namespace MusicGQL.Features.ServerLibrary.Release;

public interface IReleaseBase
{
    [ID]
    string Id();
    string Title();
    string? Date();
    string? Year();
    string? Country();
    string CoverArtUri();
}
