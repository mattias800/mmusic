using MusicGQL.Features.ServerLibrary.Artist;
using MusicGQL.Features.ServerLibrary.ReleaseGroup;

namespace MusicGQL.Features.ServerLibrary;

public record ServerLibrarySearchRoot
{
    public ArtistsInServerLibrarySearchRoot ArtistsInServerLibrary() => new();

    public ReleaseGroupsInServerLibrarySearchRoot ReleaseGroupsInServerLibrary() => new();
}
