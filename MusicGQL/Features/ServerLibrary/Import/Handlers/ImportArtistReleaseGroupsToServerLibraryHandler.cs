using MusicGQL.Features.ServerLibrary.ArtistServerStatus.Services;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;

namespace MusicGQL.Features.ServerLibrary.Import.Handlers;

public class ImportArtistReleaseGroupsToServerLibraryHandler(
    ArtistServerStatusService artistServerStatusService,
    MusicBrainzService mbService,
    ServerLibraryImporterService serverLibraryImporterService,
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

            var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(command.ArtistMbId);

            artistServerStatusService.SetImportingArtistReleasesStatus(
                command.ArtistMbId,
                0,
                releaseGroups.Count
            );

            foreach (var releaseGroup in releaseGroups)
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
                    await serverLibraryImporterService.SaveReleaseGroupInDatabase(
                        releaseGroup,
                        mainRelease
                    );
                }

                artistServerStatusService.IncreaseNumReleaseGroupsFinishedImporting(
                    command.ArtistMbId
                );
            }

            artistServerStatusService.SetReadyStatus(command.ArtistMbId);

            logger.LogInformation("Artist {ArtistMbId} saved/updated in Neo4j", command.ArtistMbId);

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
