namespace MusicGQL.Features.ServerLibrary.ArtistServerStatus;

[UnionType]
public abstract record ArtistServerStatus([property: GraphQLIgnore] string ArtistMbId)
{
    [ID]
    public string Id() => ArtistMbId;

    public abstract bool TopTracksVisible();

    public abstract bool ReleasesVisible();
}

public record ArtistServerStatusReady(string ArtistMbId) : ArtistServerStatus(ArtistMbId)
{
    public override bool TopTracksVisible() => true;

    public override bool ReleasesVisible() => true;
}

public record ArtistServerStatusImportingArtist(string ArtistMbId) : ArtistServerStatus(ArtistMbId)
{
    public override bool TopTracksVisible() => false;

    public override bool ReleasesVisible() => false;
}

public record ArtistServerStatusUpdatingArtist(string ArtistMbId) : ArtistServerStatus(ArtistMbId)
{
    public override bool TopTracksVisible() => true;

    public override bool ReleasesVisible() => true;
}

public record ArtistServerStatusImportingArtistReleases(string ArtistMbId)
    : ArtistServerStatus(ArtistMbId)
{
    public override bool TopTracksVisible() => false;

    public override bool ReleasesVisible() => false;
}

public record ArtistServerStatusUpdatingArtistReleases(string ArtistMbId)
    : ArtistServerStatus(ArtistMbId)
{
    public override bool TopTracksVisible() => true;

    public override bool ReleasesVisible() => true;
}

public record ArtistServerStatusNotInLibrary(string ArtistMbId) : ArtistServerStatus(ArtistMbId)
{
    public override bool TopTracksVisible() => false;

    public override bool ReleasesVisible() => false;
}
