using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Import.Handlers;

public class ImportReleaseGroupToServerLibraryHandler(
    MusicBrainzService mbService,
    ILogger<ImportReleaseGroupToServerLibraryHandler> logger
)
{
    public async Task Handle(Command command)
    {
        try
        {
            logger.LogInformation(
                "Importing release group, id={ReleaseGroupMbId}",
                command.ReleaseGroupMbId
            );

            var releaseGroup = await mbService.GetReleaseGroupByIdAsync(command.ReleaseGroupMbId);

            if (releaseGroup is null)
            {
                logger.LogError(
                    "Release group {MessageReleaseGroupMbId} not found in MusicBrainz",
                    command.ReleaseGroupMbId
                );
                return;
            }

            logger.LogInformation(
                "Found release group, id={ReleaseGroupMbId}: {ReleaseGroupTitle}",
                command.ReleaseGroupMbId,
                releaseGroup.Title
            );

            var artistId = releaseGroup.Credits?.First().Artist.Id;
            // removed artist server status updates

            logger.LogInformation(
                "Fetching all releases for release group {Title}",
                releaseGroup.Title
            );

            var allReleases = await mbService.GetReleasesForReleaseGroupAsync(releaseGroup.Id);

            logger.LogInformation(
                "Fetched {ReleaseCount} releases for release group {Title}",
                allReleases.Count,
                releaseGroup.Title
            );

            var mainRelease = LibraryDecider.GetMainReleaseInReleaseGroup(allReleases.ToList());

            if (mainRelease == null)
            {
                logger.LogWarning(
                    "No main release found for release group {Title} after fetching {Count} releases. Only the release group itself will be persisted",
                    releaseGroup.Title,
                    allReleases.Count
                );
                // removed artist server status updates
                return;
            }

            logger.LogInformation(
                "Prioritized main release for release group {RgTitle} is {MainReleaseTitle} ({MainReleaseId})",
                releaseGroup.Title,
                mainRelease.Title,
                mainRelease.Id
            );

            // await serverLibraryImporterService.SaveReleaseGroupInDatabase(
            //     releaseGroup,
            //     mainRelease
            // );
            // removed artist server status updates
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error while importing release group {ReleaseGroupMbId}",
                command.ReleaseGroupMbId
            );
        }
    }

    // removed artist server status updates

    public record Command(string ReleaseGroupMbId);
}
