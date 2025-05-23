namespace MusicGQL.Features.ServerLibrary.Artist;

[InterfaceType("Artist")]
public interface IArtist
{
    public string Id();
    public string Name();
    public string SortName();
}
