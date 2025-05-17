using MusicGQL.Integration.Youtube;

namespace MusicGQL.Features.Recording;

public record RecordingStreamingServiceInfo(
    [property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Recording Model
)
{
    [ID]
    public string Id => Model.Id;

    public string? YoutubeMusicUrl()
    {
        var r = Model.Relations.FirstOrDefault(r => r.Url.Resource.Contains("music.youtube.com"));
        return r?.Url.Resource;
    }

    public string? YoutubeVideoId()
    {
        var r = Model.Relations.FirstOrDefault(r => r.Url.Resource.Contains("youtube"));
        return r?.Url.Resource.Split("v=").LastOrDefault();
    }

    public async Task<string?> YoutubeSearchVideoId([Service] YouTubeService youTubeService)
    {
        var searchText = $"{Model.Credits.FirstOrDefault()?.Name ?? ""} {Model.Title}";
        return await youTubeService.GetVideoIdForSearchText(searchText);
    }
}
