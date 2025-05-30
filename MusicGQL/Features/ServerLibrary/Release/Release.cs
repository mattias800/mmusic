using Hqub.MusicBrainz.Entities;
using MusicGQL.Features.ServerLibrary.Release.Db;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Release;

public record Release([property: GraphQLIgnore] DbRelease Model) : IReleaseBase
{
    [ID]
    public string Id() => Model.Id;

    public string Title() => Model.Title;

    public string? Date() => Model.Date;

    public string? Year() => Model.Date?.Split("-").FirstOrDefault();

    //public string? Barcode() => Model.Barcode;
    public string? Country() => Model.Country;

    public string? Status() => Model.Status;

    //public string? Quality => Model.Quality;
    //public IEnumerable<Genre> Genres => Model.Genres?.Select(g => new Genre(g)) ?? [];
    // public IEnumerable<MbMedium> Media => Model.Media?.Select(m => new MbMedium(m)) ?? [];

    public async Task<ReleaseGroup.ReleaseGroup?> ReleaseGroup(ServerLibraryService service)
    {
        var releaseGroup = await service.GetReleaseGroupForReleaseAsync(Model.Id);
        return releaseGroup is null ? null : new(releaseGroup);
    }

    public string CoverArtUri() => CoverArtArchive.GetCoverArtUri(Model.Id).ToString();

    public async Task<IEnumerable<Common.NameCredit>> Credits(ServerLibraryService service)
    {
        var artistCredits = await service.GetCreditsOnReleaseAsync(Model.Id);
        return artistCredits.Select(c => new Common.NameCredit(c));
    }

    public async Task<IEnumerable<Recording.Recording>> Recordings(ServerLibraryService service)
    {
        var recordings = await service.GetRecordingsForReleaseAsync(Model.Id);
        var sortedByTrackPosition = recordings
            .OrderBy(r => r.TrackPosition)
            .Select(r => r.DbRecording)
            .ToList();

        return sortedByTrackPosition.Select(r => new Recording.Recording(r));
    }

    public async Task<IEnumerable<Common.Label>> Labels(ServerLibraryService service)
    {
        var labels = await service.GetLabelsForReleaseAsync(Model.Id);
        return labels.Select(r => new Common.Label(r));
    }
};
