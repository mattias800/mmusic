namespace MusicGQL.Features.ServerLibrary.Release;

public interface IReleaseBase
{
    string Id();
    string Title();
    string? Date();
    string? Year();
    string? Country();
    string CoverArtUri();
}
