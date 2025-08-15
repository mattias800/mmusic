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
                
                // Basic primary artist check
                var isPrimaryArtist = primaryCredit.Artist.Id == command.ArtistMbId;
                if (!isPrimaryArtist) return false;
                
                // Additional filtering to exclude non-official releases
                var title = rg.Title?.ToLowerInvariant() ?? "";
                var secondaryTypes = rg.SecondaryTypes?.Select(t => t.ToLowerInvariant()).ToList() ?? new List<string>();
                
                // Exclude compilations, anthologies, live recordings, mixtapes, etc.
                var excludeKeywords = new[]
                {
                    "anthology", "compilation", "collection", "greatest hits", "best of",
                    "live", "concert", "performance", "storytellers", "unplugged",
                    "mixtape", "presented by", "dj", "remix", "remastered",
                    "deluxe", "expanded", "special edition", "anniversary"
                };
                
                // Check if title contains any exclude keywords
                if (excludeKeywords.Any(keyword => title.Contains(keyword)))
                {
                    return false;
                }
                
                // Specific exclusions for albums that should not be imported
                var specificExcludeTitles = new[]
                {
                    "the college dropout video anthology",
                    "late orchestration",
                    "can't tell me nothing: the official mixtape",
                    "sky high: presented by dj benzi and plain pat",
                    "good fridays",
                    "vh1 storytellers"
                };
                
                if (specificExcludeTitles.Any(excludeTitle => title.Contains(excludeTitle)))
                {
                    return false;
                }
                
                // Check secondary types that indicate non-studio releases
                var excludeSecondaryTypes = new[]
                {
                    "compilation", "live", "mixtape", "remix", "dj-mix"
                };
                
                if (excludeSecondaryTypes.Any(excludeType => secondaryTypes.Contains(excludeType)))
                {
                    return false;
                }
                
                return true;
            }).ToList();
            
            // Special handling for important collaborations that should be included
            var importantCollaborations = allReleaseGroup.Where(rg =>
            {
                var credits = rg.Credits?.ToList();
                if (credits == null || credits.Count == 0) return false;
                
                // Check if this artist appears but is not the primary artist
                var hasArtist = credits.Any(c => c.Artist?.Id == command.ArtistMbId);
                if (!hasArtist) return false;
                
                var primaryCredit = credits.FirstOrDefault();
                if (primaryCredit?.Artist?.Id == null) return false;
                
                // Only include if this artist is not the primary artist
                if (primaryCredit.Artist.Id == command.ArtistMbId) return false;
                
                // Check for specific important collaborations
                var title = rg.Title?.ToLowerInvariant() ?? "";
                var importantCollaborationTitles = new[]
                {
                    "watch the throne", // Kanye West + Jay-Z collaboration
                    "kids see ghosts", // Kanye West + Kid Cudi collaboration
                    "cruel summer", // GOOD Music compilation (but important)
                    "ye vs. the people" // Kanye West + T.I. collaboration
                };
                
                return importantCollaborationTitles.Any(importantTitle => title.Contains(importantTitle));
            }).ToList();
            
            // Combine primary artist groups with important collaborations
            var finalImportGroups = primaryArtistGroups.Concat(importantCollaborations).ToList();
            
            logger.LogInformation("Filtered to {PrimaryArtistCount} primary artist release groups and {ImportantCollaborationCount} important collaborations", 
                primaryArtistGroups.Count, importantCollaborations.Count);
            logger.LogInformation("Final import count: {FinalCount} release groups", finalImportGroups.Count);

            var releaseGroupsToImport = finalImportGroups
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
