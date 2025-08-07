using MusicGQL.Features.ArtistServerStatus.Services;
using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Import.Handlers;

public class ImportArtistReleaseGroupsToServerLibraryHandler(
    ArtistServerStatusService artistServerStatusService,
    MusicBrainzService mbService,
    ILogger<ImportArtistReleaseGroupsToServerLibraryHandler> logger
)
{
    public async Task<Result> Handle(Command command)
    {
        try
        {
            logger.LogInformation(
                "Importing release groups for artist, id={Id}",
                command.ArtistMbId
            );

            var allReleaseGroup = await mbService.GetReleaseGroupsForArtistAsync(
                command.ArtistMbId
            );

            var releaseGroupsToImport = allReleaseGroup
                .Where(LibraryDecider.ShouldBeAddedWhenAddingArtistToServerLibrary)
                .ToList();

            artistServerStatusService.SetImportingArtistReleasesStatus(
                command.ArtistMbId,
                0,
                releaseGroupsToImport.Count
            );

            foreach (var releaseGroup in releaseGroupsToImport)
            {
                var allReleases = await mbService.GetReleasesForReleaseGroupAsync(releaseGroup.Id);
                var mainRelease = LibraryDecider.GetMainReleaseInReleaseGroup(allReleases.ToList());

                if (mainRelease is null)
                {
                    logger.LogWarning(
                        "No main release found for release group {ReleaseGroupId}",
                        releaseGroup.Id
                    );
                }
                else
                {
                    // await serverLibraryImporterService.SaveReleaseGroupInDatabase(
                    //     releaseGroup,
                    //     mainRelease
                    // );
                }

                artistServerStatusService.IncreaseNumReleaseGroupsFinishedImporting(
                    command.ArtistMbId
                );
            }

            artistServerStatusService.SetReadyStatus(command.ArtistMbId);

            // logger.LogInformation("Artist {ArtistMbId} saved/updated in Neo4j", command.ArtistMbId);

            return new Result.Success();
        }
        catch (Exception e)
        {
            return new Result.Error(e.Message);
        }
    }

    public record Command(string ArtistMbId);

    public abstract record Result
    {
        public record Success : Result;

        public record ArtistNotFound : Result;

        public record Error(string Message) : Result;
    }
}
