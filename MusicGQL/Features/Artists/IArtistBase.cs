namespace MusicGQL.Features.Artists;

[InterfaceType("ArtistBase")]
public interface IArtistBase
{
    [ID]
    string Id();

    string Name();

    string SortName();
}