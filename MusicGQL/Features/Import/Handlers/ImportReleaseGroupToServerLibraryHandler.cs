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
            
            // Log detailed release group information for debugging
            logger.LogInformation("Release group details:");
            logger.LogInformation("  - Title: {Title}", releaseGroup.Title);
            logger.LogInformation("  - Primary Type: {PrimaryType}", releaseGroup.PrimaryType);
            logger.LogInformation("  - First Release Date: {FirstReleaseDate}", releaseGroup.FirstReleaseDate);
            
            if (releaseGroup.Credits?.Any() == true)
            {
                logger.LogInformation("  - Credits ({CreditCount}):", releaseGroup.Credits.Count());
                foreach (var credit in releaseGroup.Credits.Take(5)) // Limit to first 5
                {
                    logger.LogInformation("    * {ArtistName} (ID: {ArtistId}, Join: '{JoinPhrase}')", 
                        credit.Name ?? credit.Artist?.Name ?? "Unknown", 
                        credit.Artist?.Id ?? "Unknown", 
                        credit.JoinPhrase ?? "");
                }
            }
            else
            {
                logger.LogWarning("  - No credits found for release group");
            }

            var artistId = releaseGroup.Credits?.First().Artist.Id;
            
            // Log artist information for debugging
            if (releaseGroup.Credits?.Any() == true)
            {
                var primaryCredit = releaseGroup.Credits.First();
                var artistName = primaryCredit.Name ?? primaryCredit.Artist?.Name ?? "Unknown";
                logger.LogInformation("Primary artist: '{ArtistName}' (ID: {ArtistId})", artistName, artistId);
                
                // Check if this is a name change situation
                if (primaryCredit.Artist != null)
                {
                    logger.LogInformation("Artist entity: Name='{ArtistName}', SortName='{SortName}', Type='{ArtistType}'", 
                        primaryCredit.Artist.Name, primaryCredit.Artist.SortName, primaryCredit.Artist.Type);
                }
            }
            else
            {
                logger.LogWarning("No artist credits found in release group");
            }
            
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
