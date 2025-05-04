using MusicGQL.Features.Release;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Artist;

public record Artist([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Artist Model)
{
    [ID]
    public string Id => Model.Id;
    public string Name => Model.Name;
    public string SortName => Model.SortName;
    public string? Disambiguation => Model.Disambiguation;
    public string? Type => Model.Type;

    public async Task<IEnumerable<Release.Release>> Releases([Service] MusicBrainzService mbService)
    {
        var releases = await mbService.GetReleasesForArtistAsync(Id);
        return releases.Select(r => new Release.Release(r));
    }

    public async Task<IEnumerable<ReleaseGroup>> ReleaseGroups(
        [Service] MusicBrainzService mbService
    )
    {
        var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(Id);
        return releaseGroups.Select(r => new ReleaseGroup(r));
    }

    public async Task<IEnumerable<Release.Release>> Albums([Service] MusicBrainzService mbService)
    {
        var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(Id);
        var albumReleaseGroups = releaseGroups.Where(r => r.IsAlbum()).ToList();
        var albums = await Task.WhenAll(
            albumReleaseGroups.Select(async rg =>
            {
                var releases = await mbService.GetReleasesForReleaseGroupAsync(rg.Id);
                return MainAlbumFinder.GetMainReleaseInReleaseGroup(releases);
            })
        );

        return albums
            .OfType<Hqub.MusicBrainz.Entities.Release>()
            .Where(r => r.Status == "Official")
            .Select(r => new Release.Release(r));
    }
}
