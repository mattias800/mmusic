using MusicGQL.Features.MusicBrainz.ReleaseGroup;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup;

public record ReleaseGroup([property: GraphQLIgnore] DbReleaseGroup Model)
{
    public string Id() => Model.Id;

    public string Title() => Model.Title;

    public string? PrimaryType() => Model.PrimaryType;

    public IEnumerable<string> SecondaryTypes() => Model.SecondaryTypes;

    public async Task<IEnumerable<Common.NameCredit>> Credits(Neo4jService service)
    {
        var artistCredits = await service.GetCreditsOnReleaseGroupAsync(Model.Id);
        return artistCredits.Select(c => new Common.NameCredit(c));
    }

    public string? FirstReleaseDate => Model.FirstReleaseDate;
    public string? FirstReleaseYear => Model.FirstReleaseDate?.Split("-").FirstOrDefault();

    public async Task<Release.Release?> MainRelease(Neo4jService service)
    {
        var all = await service.GetReleasesForReleaseGroupAsync(Model.Id);
        var best = all.FirstOrDefault();
        return best is null ? null : new Release.Release(best);
    }

    public async Task<MbReleaseGroup?> MusicBrainzReleaseGroup(
        [Service] MusicBrainzService musicBrainzService
    )
    {
        var releaseGroup = await musicBrainzService.GetReleaseGroupByIdAsync(Model.Id);

        if (releaseGroup == null)
        {
            return null;
        }

        return new(releaseGroup);
    }
};
