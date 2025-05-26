namespace MusicGQL.Features.ServerLibrary.ArtistServerStatus;

public record ArtistServerStatusSearchRoot
{
    public ArtistServerStatus ByArtistId([ID] string artistId) => new(artistId);
};
