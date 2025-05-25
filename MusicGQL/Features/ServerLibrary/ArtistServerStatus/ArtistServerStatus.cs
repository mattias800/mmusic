using MusicGQL.Features.ServerLibrary.ArtistServerStatus.Services;

namespace MusicGQL.Features.ServerLibrary.ArtistServerStatus;

public record ArtistServerStatus([property: GraphQLIgnore] string ArtistMbId)
{
    [ID]
    public string Id() => ArtistMbId;

    public async Task<ArtistServerStatusResult> Result(ArtistServerStatusService service) =>
        await service.GetArtistServerStatus(ArtistMbId);
}

[UnionType("ArtistServerStatusResult")]
public abstract record ArtistServerStatusResult
{
    public abstract bool TopTracksVisible();

    public abstract bool ReleasesVisible();
}

[InterfaceType("ArtistServerStatusResultBase")]
public interface IArtistServerStatusResult
{
    public abstract bool TopTracksVisible();

    public abstract bool ReleasesVisible();
}

public record ArtistServerStatusReady : ArtistServerStatusResult, IArtistServerStatusResult
{
    public override bool TopTracksVisible() => true;

    public override bool ReleasesVisible() => true;
}

public record ArtistServerStatusImportingArtist
    : ArtistServerStatusResult,
        IArtistServerStatusResult
{
    public override bool TopTracksVisible() => false;

    public override bool ReleasesVisible() => false;
}

public record ArtistServerStatusUpdatingArtist : ArtistServerStatusResult, IArtistServerStatusResult
{
    public override bool TopTracksVisible() => true;

    public override bool ReleasesVisible() => true;
}

public record ArtistServerStatusImportingArtistReleases(
    int NumReleaseGroupsFinishedImporting,
    int TotalNumReleaseGroupsBeingImported
) : ArtistServerStatusResult, IArtistServerStatusResult
{
    public override bool TopTracksVisible() => false;

    public override bool ReleasesVisible() => NumReleaseGroupsFinishedImporting > 0;
}

public record ArtistServerStatusUpdatingArtistReleases
    : ArtistServerStatusResult,
        IArtistServerStatusResult
{
    public override bool TopTracksVisible() => true;

    public override bool ReleasesVisible() => true;
}

public record ArtistServerStatusNotInLibrary : ArtistServerStatusResult, IArtistServerStatusResult
{
    public override bool TopTracksVisible() => false;

    public override bool ReleasesVisible() => false;
}
