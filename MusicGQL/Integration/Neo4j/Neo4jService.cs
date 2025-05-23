using MusicGQL.Features.ServerLibrary.Artist.Db;
using MusicGQL.Features.ServerLibrary.Recording.Db;
using MusicGQL.Features.ServerLibrary.Release.Db;
using MusicGQL.Features.ServerLibrary.ReleaseGroup.Db;
using Neo4j.Driver;

namespace MusicGQL.Integration.Neo4j;

public class Neo4jService(IDriver driver)
{
    // Artists
    public async Task<DbArtist?> GetArtistByIdAsync(string id)
    {
        var dbArtist = await ExecuteReadSingleAsync(
            "MATCH (a:Artist {Id: $id}) RETURN a",
            new { id },
            record => record["a"].As<DbArtist>()
        );
        return dbArtist;
    }

    public async Task<List<DbArtist>> GetArtistsForRecordingAsync(string recordingId)
    {
        // Assuming a relationship like (Artist)-[:PERFORMED_ON]->(Recording) or similar
        // This query might need adjustment based on the actual relationship name and direction.
        // The ReleaseGroupPersistenceService shows ARTIST_CREDIT_FOR_RECORDING, so let's assume that.
        var dbArtists = await ExecuteReadListAsync(
            "MATCH (a:Artist)-[:ARTIST_CREDIT_FOR_RECORDING]->(r:Recording {Id: $recordingId}) RETURN a",
            new { recordingId },
            record => record["a"].As<DbArtist>()
        );
        return dbArtists;
    }

    public async Task<List<DbArtist>> SearchArtistByNameAsync(
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var dbArtists = await ExecuteReadListAsync(
            "MATCH (a:Artist) WHERE toLower(a.Name) CONTAINS toLower($name) RETURN a ORDER BYCESIUN a.Name SKIP $offset LIMIT $limit",
            new
            {
                name,
                offset,
                limit,
            },
            record => record["a"].As<DbArtist>()
        );
        return dbArtists;
    }

    public async Task<List<DbArtist>> AllArtists(int limit = 25, int offset = 0)
    {
        var dbArtists = await ExecuteReadListAsync(
            "MATCH (a:Artist) RETURN a ORDER BYCESIUN a.Name SKIP $offset LIMIT $limit",
            new { offset, limit },
            record => record["a"].As<DbArtist>()
        );
        return dbArtists;
    }

    // Recordings
    public async Task<DbRecording?> GetRecordingByIdAsync(string id)
    {
        var dbRecording = await ExecuteReadSingleAsync(
            "MATCH (r:Recording {Id: $id}) RETURN r",
            new { id },
            record => record["r"].As<DbRecording>()
        );
        return dbRecording;
    }

    public async Task<List<DbRecording>> GetRecordingsForArtistAsync(string artistId)
    {
        // Assuming (Artist)-[:PERFORMED_ON]->(Recording) or similar
        var dbRecordings = await ExecuteReadListAsync(
            "MATCH (a:Artist {Id: $artistId})<-[:ARTIST_CREDIT_FOR_RECORDING]-(r:Recording) RETURN r",
            new { artistId },
            record => record["r"].As<DbRecording>()
        );
        return dbRecordings;
    }

    public async Task<List<DbRecording>> GetRecordingsForReleaseAsync(string releaseId)
    {
        // Assuming (Release)-[:HAS_TRACK]->(Recording) or (Medium)-[:INCLUDES_TRACK]->(Recording)
        // and (Release)-[:HAS_MEDIUM]->(Medium)
        var dbRecordings = await ExecuteReadListAsync(
            "MATCH (rel:Release {Id: $releaseId})-[:HAS_MEDIUM]->(m:Medium)-[:INCLUDES_TRACK]->(r:Recording) RETURN DISTINCT r",
            new { releaseId },
            record => record["r"].As<DbRecording>()
        );
        return dbRecordings;
    }

    public async Task<List<DbRecording>> SearchRecordingByNameAsync(
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var dbRecordings = await ExecuteReadListAsync(
            "MATCH (r:Recording) WHERE toLower(r.Title) CONTAINS toLower($name) RETURN r ORDER BY r.Title SKIP $offset LIMIT $limit",
            new
            {
                name,
                offset,
                limit,
            },
            record => record["r"].As<DbRecording>()
        );
        return dbRecordings;
    }

    // Releases
    public async Task<DbRelease?> GetReleaseByIdAsync(string releaseId)
    {
        var dbRelease = await ExecuteReadSingleAsync(
            "MATCH (rel:Release {Id: $releaseId}) RETURN rel",
            new { releaseId },
            record => record["rel"].As<DbRelease>()
        );
        return dbRelease;
    }

    public async Task<List<DbRelease>> GetReleasesForArtistAsync(string artistId)
    {
        // This is a bit more complex as an artist can be credited on a release or its release group.
        // Focusing on direct credits to Release first. The relationship is likely (Artist)-[:ARTIST_CREDIT_FOR_RELEASE]->(Release)
        var dbReleases = await ExecuteReadListAsync(
            "MATCH (a:Artist {Id: $artistId})<-[:ARTIST_CREDIT_FOR_RELEASE]-(rel:Release) RETURN rel",
            new { artistId },
            record => record["rel"].As<DbRelease>()
        );
        return dbReleases;
    }

    public async Task<List<DbRelease>> GetReleasesForRecordingAsync(string recordingId)
    {
        // (Recording)<-[:INCLUDES_TRACK]-(Medium)<-[:HAS_MEDIUM]-(Release)
        var dbReleases = await ExecuteReadListAsync(
            "MATCH (rec:Recording {Id: $recordingId})<-[:INCLUDES_TRACK]-(m:Medium)<-[:HAS_MEDIUM]-(rel:Release) RETURN DISTINCT rel",
            new { recordingId },
            record => record["rel"].As<DbRelease>()
        );
        return dbReleases;
    }

    public async Task<List<DbRelease>> GetReleasesForReleaseGroupAsync(string releaseGroupId)
    {
        var dbReleases = await ExecuteReadListAsync(
            "MATCH (rg:ReleaseGroup {Id: $releaseGroupId})<-[:RELEASE_OF]-(rel:Release) RETURN rel",
            new { releaseGroupId },
            record => record["rel"].As<DbRelease>()
        );
        return dbReleases;
    }

    public async Task<List<DbRelease>> SearchReleaseByNameAsync(
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var dbReleases = await ExecuteReadListAsync(
            "MATCH (rel:Release) WHERE toLower(rel.Title) CONTAINS toLower($name) RETURN rel ORDER BY rel.Title SKIP $offset LIMIT $limit",
            new
            {
                name,
                offset,
                limit,
            },
            record => record["rel"].As<DbRelease>()
        );
        return dbReleases;
    }

    // Release groups
    public async Task<DbReleaseGroup?> GetReleaseGroupByIdAsync(string releaseGroupId)
    {
        var dbReleaseGroup = await ExecuteReadSingleAsync(
            "MATCH (rg:ReleaseGroup {Id: $releaseGroupId}) RETURN rg",
            new { releaseGroupId },
            record => record["rg"].As<DbReleaseGroup>()
        );
        return dbReleaseGroup;
    }

    public async Task<List<DbReleaseGroup>> SearchReleaseGroupByNameAsync(
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var dbReleaseGroups = await ExecuteReadListAsync(
            "MATCH (rg:ReleaseGroup) WHERE toLower(rg.Title) CONTAINS toLower($name) RETURN rg ORDER BY rg.Title SKIP $offset LIMIT $limit",
            new
            {
                name,
                offset,
                limit,
            },
            record => record["rg"].As<DbReleaseGroup>()
        );
        return dbReleaseGroups;
    }

    public async Task<List<DbReleaseGroup>> GetReleaseGroupsForArtistAsync(string artistId)
    {
        // Assuming (Artist)-[:ARTIST_CREDIT_FOR_RELEASE_GROUP]->(ReleaseGroup)
        var dbReleaseGroups = await ExecuteReadListAsync(
            "MATCH (a:Artist {Id: $artistId})<-[:ARTIST_CREDIT_FOR_RELEASE_GROUP]-(rg:ReleaseGroup) RETURN rg",
            new { artistId },
            record => record["rg"].As<DbReleaseGroup>()
        );
        return dbReleaseGroups;
    }

    private async Task<T?> ExecuteReadSingleAsync<T>(
        string query,
        object parameters,
        Func<IRecord, T> mapper
    )
    {
        await using var session = driver.AsyncSession();
        var result = await session.RunAsync(query, parameters);
        var record = await result.SingleAsync();
        return record != null ? mapper(record) : default;
    }

    private async Task<List<T>> ExecuteReadListAsync<T>(
        string query,
        object parameters,
        Func<IRecord, T> mapper
    )
    {
        await using var session = driver.AsyncSession();
        var result = await session.RunAsync(query, parameters);
        return await result.ToListAsync(mapper);
    }
}
