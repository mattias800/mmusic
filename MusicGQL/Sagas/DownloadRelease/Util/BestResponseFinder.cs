using Soulseek;

namespace MusicGQL.Sagas.DownloadRelease.Util;

public static class BestResponseFinder
{
    public static SearchResponse? GetBestSearchResponse(
        ICollection<SearchResponse> searchResponses
    ) =>
        searchResponses
            .Where(r => r.FileCount > 0)
            .Where(HasCorrectMediaType)
            .OrderBy(r => r.QueueLength)
            .ThenBy(r => r.HasFreeUploadSlot)
            .ThenByDescending(r => r.UploadSpeed)
            .FirstOrDefault();

    public static bool HasCorrectMediaType(SearchResponse response)
    {
        var file = response.Files.First();
        return file.Extension.ToLower() == "mp3" && file.BitRate == 320;
    }
}
