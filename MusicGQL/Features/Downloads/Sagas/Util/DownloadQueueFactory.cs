using Soulseek;

namespace MusicGQL.Features.Downloads.Sagas.Util;

public static class DownloadQueueFactory
{
    public static Queue<DownloadQueueItem> Create(SearchResponse searchResponse) =>
        new(
            searchResponse
                .Files.Select(
                    (file, i) =>
                        new DownloadQueueItem(
                            searchResponse.Username,
                            file.Filename,
                            $"./song_{i}.mp3"
                        )
                )
                .ToList()
        );
}
