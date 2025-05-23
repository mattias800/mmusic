using MusicGQL.Common;
using MusicGQL.Features.MusicBrainz.ReleaseGroup;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup;

public record ReleaseGroup([property: GraphQLIgnore] DbReleaseGroup Model)
{
    public string Id => Model.Id;
    public string Title => Model.Title;
    public string? PrimaryType => Model.PrimaryType;
    public IEnumerable<string> SecondaryTypes => Model.SecondaryTypes;

    //public IEnumerable<Common.NameCredit> Credits() =>
    //    Model.Credits?.Select(c => new NameCredit(c)) ?? [];

    public string? FirstReleaseDate => Model.FirstReleaseDate;
    public string? FirstReleaseYear => Model.FirstReleaseDate?.Split("-").FirstOrDefault();

    public async Task<Release.Release?> MainRelease(Neo4jService service)
    {
        var all = await service.GetReleasesForReleaseGroupAsync(Id);
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
