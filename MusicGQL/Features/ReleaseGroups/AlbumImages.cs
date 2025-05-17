namespace MusicGQL.Features.ReleaseGroups;

public record AlbumImages(
    [property: GraphQLIgnore] TrackSeries.FanArtTV.Client.Music.AlbumImages Model
)
{
    public string? AlbumCover => Model.AlbumCover?.FirstOrDefault()?.Url;
    public string? CdArt => Model.CdArt?.FirstOrDefault()?.Url;
}
