using FanartArtistImages = TrackSeries.FanArtTV.Client.Music.ArtistImages;

namespace MusicGQL.Features.Artist;

public record ArtistImages([property: GraphQLIgnore] FanartArtistImages Model)
{
    public string? ArtistBackground => Model.ArtistBackground?.FirstOrDefault()?.Url;
    public string? ArtistThumb => Model.ArtistThumb?.FirstOrDefault()?.Url;
    public string? MusicLogo => Model.MusicLogo?.FirstOrDefault()?.Url;
    public string? HDMusicLogo => Model.HDMusicLogo?.FirstOrDefault()?.Url;
    public string? MusicBanner => Model.MusicBanner?.FirstOrDefault()?.Url;
}