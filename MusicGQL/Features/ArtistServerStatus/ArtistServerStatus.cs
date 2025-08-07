using MusicGQL.Features.ArtistServerStatus.Services;
using MusicGQL.Features.ServerLibrary;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.ArtistServerStatus;

public record ArtistServerStatus([property: GraphQLIgnore] string ArtistMbId)
{
    [ID]
    public string Id() => ArtistMbId;

    public async Task<ArtistServerStatusResult> Result(ArtistServerStatusService service) =>
        await service.GetArtistServerStatus(ArtistMbId);
}

[UnionType("ArtistServerStatusResult")]
public abstract record ArtistServerStatusResult([property: GraphQLIgnore] string ArtistId)
{
    public abstract bool TopTracksVisible();

    public abstract bool ReleasesVisible();
}

[InterfaceType("ArtistServerStatusResultBase")]
public interface IArtistServerStatusResult
{
    public bool TopTracksVisible();

    public bool ReleasesVisible();
}

public record ArtistServerStatusReady(string ArtistMbId)
    : ArtistServerStatusResult(ArtistMbId),
        IArtistServerStatusResult
{
    public override bool TopTracksVisible() => true;

    public override bool ReleasesVisible() => true;
}

public record ArtistServerStatusImportingArtist(string ArtistMbId)
    : ArtistServerStatusResult(ArtistMbId),
        IArtistServerStatusResult
{
    public override bool TopTracksVisible() => false;

    public override bool ReleasesVisible() => false;
}

public record ArtistServerStatusUpdatingArtist(string ArtistMbId)
    : ArtistServerStatusResult(ArtistMbId),
        IArtistServerStatusResult
{
    public override bool TopTracksVisible() => true;

    public override bool ReleasesVisible() => true;
}

public record ArtistServerStatusImportingArtistReleases(
    string ArtistId,
    int NumReleaseGroupsFinishedImporting,
    int TotalNumReleaseGroupsBeingImported
) : ArtistServerStatusResult(ArtistId), IArtistServerStatusResult
{
    public override bool TopTracksVisible() => false;

    public override bool ReleasesVisible() => NumReleaseGroupsFinishedImporting > 0;

    public async Task<Artist> Artist(ServerLibraryCache cache)
    {
        var artist = await cache.GetArtistByIdAsync(ArtistId);
        if (artist is null)
        {
            throw new Exception("Artist not found");
        }

        return new Artist(artist);
    }
}

public record ArtistServerStatusUpdatingArtistReleases(string ArtistMbId)
    : ArtistServerStatusResult(ArtistMbId),
        IArtistServerStatusResult
{
    public override bool TopTracksVisible() => true;

    public override bool ReleasesVisible() => true;
}

public record ArtistServerStatusNotInLibrary(string ArtistMbId)
    : ArtistServerStatusResult(ArtistMbId),
        IArtistServerStatusResult
{
    public override bool TopTracksVisible() => false;

    public override bool ReleasesVisible() => false;
}
