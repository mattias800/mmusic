using MusicGQL.Db.Postgres;
using MusicGQL.Features.ServerLibrary.Events;
using MusicGQL.Integration.MusicBrainz;

namespace MusicGQL.Features.ServerLibrary.Artist.Handlers;

public class MarkArtistReleaseGroupsAsAddedToServerLibraryHandler(
    EventDbContext dbContext,
    EventProcessor.EventProcessorWorker eventProcessorWorker,
    MusicBrainzService mbService,
    ILogger<MarkArtistReleaseGroupsAsAddedToServerLibraryHandler> logger
)
{
    public async Task Handle(Command command)
    {
        try
        {
            var artist = await mbService.GetArtistByIdAsync(command.ArtistId);

            if (artist is null)
            {
                return;
            }

            logger.LogInformation(
                "Marking release groups for artist {Name} as added to server library",
                artist.Name
            );

            var releaseGroups = await mbService.GetReleaseGroupsForArtistAsync(artist.Id);

            var releaseGroupIds = releaseGroups
                .Where(LibraryDecider.ShouldBeAddedWhenAddingArtistToServerLibrary)
                .Select(r => r.Id)
                .ToList();

            logger.LogInformation(
                "Found {Count} release groups associated with artist {Name}. Marking them...",
                releaseGroupIds.Count,
                artist.Name
            );

            foreach (var releaseGroupId in releaseGroupIds)
            {
                dbContext.Events.Add(
                    new AddReleaseGroupToServerLibrary { ReleaseGroupMbId = releaseGroupId }
                );
            }

            await dbContext.SaveChangesAsync();
            await eventProcessorWorker.ProcessEvents();
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error while marking release groups for artist {ArtistMbId}",
                command.ArtistId
            );
        }
    }

    public record Command(string ArtistId);
}
