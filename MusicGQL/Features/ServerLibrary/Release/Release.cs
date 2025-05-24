using Hqub.MusicBrainz.Entities;
using MusicGQL.Features.ServerLibrary.Release.Db;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Release;

public record Release([property: GraphQLIgnore] DbRelease Model)
{
    public string Id => Model.Id;
    public string Title => Model.Title;
    public string? Date => Model.Date;

    public string? Year => Model.Date?.Split("-").FirstOrDefault();

    //public string? Barcode => Model.Barcode;
    //public string? Country => Model.Country;
    public string? Status => Model.Status;

    //public string? Quality => Model.Quality;
    //public IEnumerable<Genre> Genres => Model.Genres?.Select(g => new Genre(g)) ?? [];
    // public IEnumerable<MbMedium> Media => Model.Media?.Select(m => new MbMedium(m)) ?? [];

    // public ReleaseGroup.ReleaseGroup? ReleaseGroup =>
    //     Model.ReleaseGroup is null ? null : new(Model.ReleaseGroup);

    public string CoverArtUri => CoverArtArchive.GetCoverArtUri(Model.Id).ToString();

    public async Task<IEnumerable<Common.NameCredit>> Credits(Neo4jService service)
    {
        var artistCredits = await service.GetCreditsOnReleaseAsync(Model.Id);
        return artistCredits.Select(c => new Common.NameCredit(c));
    }

    public async Task<IEnumerable<Recording.Recording>> Recordings(Neo4jService service)
    {
        var recordings = await service.GetRecordingsForReleaseAsync(Id);
        return recordings.Select(r => new Recording.Recording(r));
    }
};
