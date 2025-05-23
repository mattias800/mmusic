namespace MusicGQL.Features.MusicBrainz.ReleaseGroup;

public record MbAlbumImages(
    [property: GraphQLIgnore] TrackSeries.FanArtTV.Client.Music.AlbumImages Model
)
{
    public string? AlbumCover => Model.AlbumCover?.FirstOrDefault()?.Url;
    public string? CdArt => Model.CdArt?.FirstOrDefault()?.Url;
}
