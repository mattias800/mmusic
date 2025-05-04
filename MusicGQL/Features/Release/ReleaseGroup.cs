using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Release;

public record ReleaseGroup([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.ReleaseGroup Model)
{
    [ID] public string Id => Model.Id;
    public string Title => Model.Title;
    public string? PrimaryType => Model.PrimaryType;
    public IEnumerable<string> SecondaryTypes => Model.SecondaryTypes;
    public string? FirstReleaseDate => Model.FirstReleaseDate;

    public async Task<Release?> MainRelease([Service] MusicBrainzService mbService)
    {
        var all = await mbService.GetReleasesForReleaseGroupAsync(Id);
        var best = MainAlbumFinder.GetMainReleaseInReleaseGroup(all.ToList());
        return best is null ? null : new Release(best);
    }
}