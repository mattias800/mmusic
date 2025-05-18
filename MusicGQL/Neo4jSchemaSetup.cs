using Neo4j.Driver;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MusicGQL;

public static class Neo4JSchemaSetup
{
    public static async Task EnsureConstraintsAsync(
        IServiceProvider serviceProvider,
        ILogger logger
    )
    {
        using var scope = serviceProvider.CreateScope();
        var driver = scope.ServiceProvider.GetRequiredService<IDriver>();

        var constraints = new List<string>
        {
            "CREATE CONSTRAINT artist_id_unique IF NOT EXISTS FOR (a:Artist) REQUIRE a.Id IS UNIQUE",
            "CREATE CONSTRAINT release_group_id_unique IF NOT EXISTS FOR (rg:ReleaseGroup) REQUIRE rg.Id IS UNIQUE",
            "CREATE CONSTRAINT release_id_unique IF NOT EXISTS FOR (r:Release) REQUIRE r.Id IS UNIQUE",
            "CREATE CONSTRAINT recording_id_unique IF NOT EXISTS FOR (rec:Recording) REQUIRE rec.Id IS UNIQUE",
        };

        logger.LogInformation("Ensuring Neo4j constraints...");
        await using var session = driver.AsyncSession();
        foreach (var constraintCypher in constraints)
        {
            try
            {
                await session.ExecuteWriteAsync(async tx => await tx.RunAsync(constraintCypher));
                logger.LogInformation(
                    "Successfully applied/verified Neo4j constraint: {ConstraintQuery}",
                    constraintCypher.Split(" FOR")[0]
                );
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to apply Neo4j constraint: {ConstraintQuery}",
                    constraintCypher.Split(" FOR")[0]
                );
                // Consider if you want to throw here to stop app startup if constraints are critical
            }
        }

        logger.LogInformation("Neo4j constraint check completed");
    }
}
