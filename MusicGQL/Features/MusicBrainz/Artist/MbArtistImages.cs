using FanartArtistImages = TrackSeries.FanArtTV.Client.Music.ArtistImages;

namespace MusicGQL.Features.MusicBrainz.Artist;

public record MbArtistImages([property: GraphQLIgnore] FanartArtistImages Model)
{
    public string? ArtistBackground => Model.ArtistBackground?.FirstOrDefault()?.Url;
    public string? ArtistThumb => Model.ArtistThumb?.FirstOrDefault()?.Url;
    public string? MusicLogo => Model.MusicLogo?.FirstOrDefault()?.Url;
    public string? HDMusicLogo => Model.HDMusicLogo?.FirstOrDefault()?.Url;
    public string? MusicBanner => Model.MusicBanner?.FirstOrDefault()?.Url;
}
