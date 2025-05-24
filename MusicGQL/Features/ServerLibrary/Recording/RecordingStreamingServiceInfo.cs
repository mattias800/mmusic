using MusicGQL.Features.ServerLibrary.Common.Db;
using MusicGQL.Features.ServerLibrary.Recording.Db;
using MusicGQL.Integration.Youtube;

namespace MusicGQL.Features.ServerLibrary.Recording;

public record RecordingStreamingServiceInfo(
    [property: GraphQLIgnore] DbRecording Model,
    [property: GraphQLIgnore] List<DbRelation> Relations,
    [property: GraphQLIgnore] List<DbNameCredit> Credits
)
{
    [ID]
    public string Id => Model.Id;

    public string? YoutubeMusicUrl()
    {
        var r = Relations.FirstOrDefault(r => r.Url.Contains("music.youtube.com"));
        return r?.Url;
    }

    public string? YoutubeVideoId()
    {
        var r = Relations.FirstOrDefault(r => r.Url.Contains("youtube"));
        return r?.Url.Split("v=").LastOrDefault();
    }

    public async Task<string?> YoutubeSearchVideoId([Service] YouTubeService youTubeService)
    {
        var searchText = $"{Credits.FirstOrDefault()?.Name ?? ""} {Model.Title}";
        return await youTubeService.GetVideoIdForSearchText(searchText);
    }
}
