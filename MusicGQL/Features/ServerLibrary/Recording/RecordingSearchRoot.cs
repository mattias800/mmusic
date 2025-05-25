using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Recording;

public record RecordingSearchRoot
{
    public async Task<IEnumerable<Recording>> SearchByName(
        ServerLibraryImportService service,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var releases = await service.SearchRecordingByNameAsync(name, limit, offset);
        return releases.Select(a => new Recording(a));
    }

    public async Task<Recording?> ById(ServerLibraryImportService service, [ID] string id)
    {
        try
        {
            var recording = await service.GetRecordingByIdAsync(id);
            return recording != null ? new Recording(recording) : null;
        }
        catch
        {
            return null;
        }
    }
};
