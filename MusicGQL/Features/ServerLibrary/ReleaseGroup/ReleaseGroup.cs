using Hqub.MusicBrainz.Entities;
using MusicGQL.Features.MusicBrainz.ReleaseGroup;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;
using TrackSeries.FanArtTV.Client;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup;

public record ReleaseGroup([property: GraphQLIgnore] DbReleaseGroup Model)
{
    public string Id() => Model.Id;

    public string Title() => Model.Title;

    public string? PrimaryType() => Model.PrimaryType;

    public IEnumerable<string> SecondaryTypes() => Model.SecondaryTypes;

    public async Task<IEnumerable<Common.NameCredit>> Credits(ServerLibraryImportService service)
    {
        var artistCredits = await service.GetCreditsOnReleaseGroupAsync(Model.Id);
        return artistCredits.Select(c => new Common.NameCredit(c));
    }

    public string? FirstReleaseDate => Model.FirstReleaseDate;
    public string? FirstReleaseYear => Model.FirstReleaseDate?.Split("-").FirstOrDefault();

    public async Task<Release.Release?> MainRelease(ServerLibraryImportService service)
    {
        var all = await service.GetReleasesForReleaseGroupAsync(Model.Id);
        var best = all.FirstOrDefault();
        return best is null ? null : new Release.Release(best);
    }

    public async Task<string?> CoverArtUri(
        IFanArtTVClient fanartClient,
        ServerLibraryImportService service
    )
    {
        var artistCredits = await service.GetCreditsOnReleaseGroupAsync(Model.Id);

        try
        {
            var artistInfo = await fanartClient.Music.GetAlbumAsync(
                artistCredits.First().Artist.Id
            );

            var m = artistInfo.Albums.GetValueOrDefault(Guid.Parse(Model.Id));

            if (m is not null)
            {
                var coverArt = m.AlbumCover?.FirstOrDefault()?.Url;
                if (coverArt is not null)
                {
                    return coverArt;
                }
            }

            var all = await service.GetReleasesForReleaseGroupAsync(Model.Id);
            var best = all.FirstOrDefault(); // Neo4j only stores main releases.

            return best is null ? null : CoverArtArchive.GetCoverArtUri(best.Id).ToString();
        }
        catch
        {
            return null;
        }
    }

    public async Task<MbReleaseGroup?> MusicBrainzReleaseGroup(
        MusicBrainzService musicBrainzService
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
