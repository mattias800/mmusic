using Hqub.MusicBrainz.Entities;
using MusicGQL.Features.MusicBrainz.Artist;
using MusicGQL.Features.MusicBrainz.Common;
using MusicGQL.Features.MusicBrainz.ReleaseGroup;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.MusicBrainz.Release;

public record MbRelease([property: GraphQLIgnore] Hqub.MusicBrainz.Entities.Release Model)
{
    [ID]
    public string Id => Model.Id;
    public string Title => Model.Title;
    public string? Date => Model.Date;

    public string? Year => Model.Date?.Split("-").FirstOrDefault();

    public string? Barcode => Model.Barcode;
    public string? Country => Model.Country;
    public string? Status => Model.Status;
    public string? Quality => Model.Quality;
    public IEnumerable<MbGenre> Genres => Model.Genres?.Select(g => new MbGenre(g)) ?? [];
    public IEnumerable<MbMedium> Media => Model.Media?.Select(m => new MbMedium(m)) ?? [];

    public MbReleaseGroup? ReleaseGroup =>
        Model.ReleaseGroup is null ? null : new(Model.ReleaseGroup);

    public string CoverArtUri => CoverArtArchive.GetCoverArtUri(Model.Id).ToString();

    public IEnumerable<MbNameCredit> Credits => Model.Credits.Select(c => new MbNameCredit(c));

    public IEnumerable<MbArtist> Artists() => Model.Credits.Select(a => new MbArtist(a.Artist));

    public async Task<IEnumerable<Recording.MbRecording>> Recordings(
        [Service] MusicBrainzService mbService
    )
    {
        var recordings = await mbService.GetRecordingsForReleaseAsync(Id);
        return recordings.Select(r => new Recording.MbRecording(r));
    }
}
