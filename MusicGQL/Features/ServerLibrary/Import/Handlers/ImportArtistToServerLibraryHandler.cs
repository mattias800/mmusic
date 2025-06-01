using HotChocolate.Subscriptions;
using MusicGQL.Integration.MusicBrainz;
using MusicGQL.Integration.Neo4j;
using Neo4j.Driver;

namespace MusicGQL.Features.ServerLibrary.Import.Handlers;

public class ImportArtistToServerLibraryHandler(
    IDriver driver,
    MusicBrainzService mbService,
    ServerLibraryImporterService serverLibraryImporterService,
    ILogger<ImportArtistToServerLibraryHandler> logger
)
{
    public async Task<Result> Handle(Command command)
    {
        try
        {
            logger.LogInformation("Importing artist, id={Id}", command.ArtistMbId);

            var artist = await mbService.GetArtistByIdAsync(command.ArtistMbId);

            if (artist is null)
            {
                logger.LogInformation(
                    "Artist {ArtistMbId} not found in MusicBrainz",
                    command.ArtistMbId
                );
                return new Result.ArtistNotFound();
            }

            await using var session = driver.AsyncSession();
            await session.ExecuteWriteAsync(async tx =>
            {
                await serverLibraryImporterService.SaveArtistNodeAsync(
                    (IAsyncTransaction)tx,
                    artist
                );
            });

            logger.LogInformation("Artist {ArtistMbId} saved/updated in Neo4j", artist.Id);

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
