using MusicGQL.Features.ServerLibrary.Utils;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.Import.Handlers;

public class ImportArtistReleaseGroupsToServerLibraryHandler(
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

            // Filter to only include release groups where this artist is the primary credited artist
            var primaryArtistGroups = allReleaseGroup.Where(rg =>
            {
                var credits = rg.Credits?.ToList();
                if (credits == null || credits.Count == 0) return false;
                
                // Check if the first (primary) credit is for this artist
                var primaryCredit = credits.FirstOrDefault();
                if (primaryCredit?.Artist?.Id == null) return false;
                
                return primaryCredit.Artist.Id == command.ArtistMbId;
            }).ToList();

            var releaseGroupsToImport = primaryArtistGroups
                .Where(LibraryDecider.ShouldBeAddedWhenAddingArtistToServerLibrary)
                .ToList();

            // removed artist server status updates

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

                // removed artist server status updates
            }

            // removed artist server status updates

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
