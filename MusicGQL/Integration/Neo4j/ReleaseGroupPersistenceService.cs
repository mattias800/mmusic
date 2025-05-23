using AutoMapper;
using Hqub.MusicBrainz.Entities;
using Neo4j.Driver;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Service;

public class ReleaseGroupPersistenceService(IMapper mapper)
{
    public async Task SaveReleaseGroupNodeAsync(
        IAsyncTransaction tx,
        Db.DbReleaseGroup dbReleaseGroupToSave
    )
    {
        await tx.RunAsync(
            "MERGE (rg:ReleaseGroup {Id: $id}) "
                + "ON CREATE SET rg.Title = $title, rg.PrimaryType = $primaryType, rg.SecondaryTypes = $secondaryTypes, rg.FirstReleaseDate = $firstReleaseDate "
                + "ON MATCH SET rg.Title = $title, rg.PrimaryType = $primaryType, rg.SecondaryTypes = $secondaryTypes, rg.FirstReleaseDate = $firstReleaseDate",
            new
            {
                id = dbReleaseGroupToSave.Id,
                title = dbReleaseGroupToSave.Title,
                primaryType = dbReleaseGroupToSave.PrimaryType,
                secondaryTypes = dbReleaseGroupToSave.SecondaryTypes,
                firstReleaseDate = dbReleaseGroupToSave.FirstReleaseDate,
            }
        );
    }

    public async Task SaveArtistNodeAsync(IAsyncTransaction tx, Artist.Db.DbArtist dbArtistToSave)
    {
        await tx.RunAsync(
            "MERGE (a:Artist {Id: $artistId}) ON CREATE SET a.Name = $name, a.SortName = $sortName, a.Gender = $gender "
                + "ON MATCH SET a.Name = $name, a.SortName = $sortName, a.Gender = $gender",
            new
            {
                artistId = dbArtistToSave.Id,
                name = dbArtistToSave.Name,
                sortName = dbArtistToSave.SortName,
                gender = dbArtistToSave.Gender,
            }
        );
    }

    public async Task SaveArtistCreditsForParentAsync(
        IAsyncTransaction tx,
        string parentEntityId,
        IEnumerable<NameCredit> creditDtos,
        string parentLabel,
        string parentIdQueryKey,
        string relationshipType
    )
    {
        foreach (var creditDto in creditDtos)
        {
            if (creditDto.Artist == null || string.IsNullOrEmpty(creditDto.Artist.Id))
                continue;

            var artistToSave = mapper.Map<Artist.Db.DbArtist>(creditDto.Artist);
            await SaveArtistNodeAsync(tx, artistToSave);

            string query =
                $"MATCH (p:{parentLabel} {{Id: ${parentIdQueryKey}}}), (a:Artist {{Id: $artistId}}) "
                + $"MERGE (a)-[r:{relationshipType}]->(p) ON CREATE SET r.joinPhrase = $joinPhrase";

            var parameters = new Dictionary<string, object>
            {
                { parentIdQueryKey, parentEntityId },
                { "artistId", artistToSave.Id },
                { "joinPhrase", creditDto.JoinPhrase ?? string.Empty },
            };
            await tx.RunAsync(query, parameters);
        }
    }

    public async Task SaveReleaseNodeAsync(
        IAsyncTransaction tx,
        Release.Db.DbRelease dbReleaseToSave
    )
    {
        await tx.RunAsync(
            "MERGE (r:Release {Id: $id}) "
                + "ON CREATE SET r.Title = $title, r.Date = $date, r.Status = $status "
                + "ON MATCH SET r.Title = $title, r.Date = $date, r.Status = $status",
            new
            {
                id = dbReleaseToSave.Id,
                title = dbReleaseToSave.Title,
                date = dbReleaseToSave.Date,
                status = dbReleaseToSave.Status,
            }
        );
    }

    public async Task LinkReleaseToReleaseGroupAsync(
        IAsyncTransaction tx,
        string rgId,
        string releaseId
    )
    {
        await tx.RunAsync(
            "MATCH (rg:ReleaseGroup {Id: $rgId}), (r:Release {Id: $releaseId}) "
                + "MERGE (r)-[:RELEASE_OF]->(rg)",
            new { rgId, releaseId }
        );
    }

    public async Task SaveMediumNodeAsync(
        IAsyncTransaction tx,
        string mediumNodeId,
        string releaseMbId,
        Medium mediumDto
    )
    {
        await tx.RunAsync(
            "MERGE (m:Medium {Id: $mediumId}) "
                + "ON CREATE SET m.Position = $position, m.Format = $format, m.TrackCount = $trackCount "
                + "ON MATCH SET m.Position = $position, m.Format = $format, m.TrackCount = $trackCount",
            new
            {
                mediumId = mediumNodeId,
                position = mediumDto.Position,
                format = mediumDto.Format ?? string.Empty,
                trackCount = mediumDto.TrackCount,
            }
        );

        // Link Medium to its Release
        await tx.RunAsync(
            "MATCH (rel:Release {Id: $releaseId}), (m:Medium {Id: $mediumId}) "
                + "MERGE (rel)-[:HAS_MEDIUM]->(m)",
            new { releaseId = releaseMbId, mediumId = mediumNodeId }
        );
    }

    public async Task SaveRecordingNodeAsync(
        IAsyncTransaction tx,
        Recording.Db.DbRecording dbRecordingToSave
    )
    {
        await tx.RunAsync(
            "MERGE (rec:Recording {Id: $id}) "
                + "ON CREATE SET rec.Title = $title, rec.Length = $length, rec.Disambiguation = $disambiguation "
                + "ON MATCH SET rec.Title = $title, rec.Length = $length, rec.Disambiguation = $disambiguation",
            new
            {
                id = dbRecordingToSave.Id,
                title = dbRecordingToSave.Title,
                length = dbRecordingToSave.Length,
                disambiguation = dbRecordingToSave.Disambiguation ?? string.Empty,
            }
        );
    }

    public async Task LinkTrackOnMediumToRecordingAsync(
        IAsyncTransaction tx,
        string mediumNodeId,
        string recordingMbId,
        Track trackDto
    )
    {
        await tx.RunAsync(
            "MATCH (m:Medium {Id: $mediumId}), (rec:Recording {Id: $recordingId}) "
                + "MERGE (m)-[r:INCLUDES_TRACK {Position: $trackPos, Number: $trackNum, Title: $trackTitle}]->(rec)",
            new
            {
                mediumId = mediumNodeId,
                recordingId = recordingMbId,
                trackPos = trackDto.Position,
                trackNum = trackDto.Number ?? string.Empty,
                trackTitle = trackDto.Recording?.Title ?? string.Empty,
            }
        );
    }
}
