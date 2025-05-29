namespace MusicGQL.Features.ServerLibrary.ReleaseGroup;

[InterfaceType("ReleaseGroupBase")]
public interface IReleaseGroupBase
{
    [ID]
    string Id();
    string Title();
    string? PrimaryType();

    IEnumerable<string> SecondaryTypes();

    //Task<IEnumerable<Common.NameCredit>> Credits(ServerLibraryService service);
    string? FirstReleaseDate();
    string? FirstReleaseYear();
}
