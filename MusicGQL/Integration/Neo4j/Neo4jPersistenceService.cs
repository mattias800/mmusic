using AutoMapper;
using Hqub.MusicBrainz.Entities;
using Neo4j.Driver;

namespace MusicGQL.Integration.Neo4j;

public class Neo4jPersistenceService(IMapper mapper)
{
    public async Task SaveReleaseGroupNodeAsync(
        IAsyncTransaction tx,
        Features.ServerLibrary.ReleaseGroup.Db.DbReleaseGroup dbReleaseGroupToSave
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

    public async Task SaveArtistNodeAsync(
        IAsyncTransaction tx,
        Features.ServerLibrary.Artist.Db.DbArtist dbArtistToSave
    )
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

            var artistToSave = mapper.Map<Features.ServerLibrary.Artist.Db.DbArtist>(
                creditDto.Artist
            );
            await SaveArtistNodeAsync(tx, artistToSave);

            string query =
                $"MATCH (p:{parentLabel} {{Id: ${parentIdQueryKey}}}), (a:Artist {{Id: $ArtistId}}) "
                + $"MERGE (a)-[r:{relationshipType}]->(p) "
                + $"ON CREATE SET r.JoinPhrase = $JoinPhrase, r.Name = $Name "
                + $"ON MATCH SET r.JoinPhrase = $JoinPhrase, r.Name = $Name";

            var parameters = new Dictionary<string, object>
            {
                { parentIdQueryKey, parentEntityId },
                { "ArtistId", artistToSave.Id },
                { "JoinPhrase", creditDto.JoinPhrase ?? string.Empty },
                { "Name", creditDto.Name ?? string.Empty },
            };

            await tx.RunAsync(query, parameters);
        }
    }

    public async Task SaveReleaseNodeAsync(
        IAsyncTransaction tx,
        Features.ServerLibrary.Release.Db.DbRelease dbReleaseToSave
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

    public async Task SaveRecordingNodeAsync(IAsyncTransaction tx, Recording recordingDtoToSave)
    {
        await tx.RunAsync(
            "MERGE (rec:Recording {Id: $id}) "
                + "ON CREATE SET rec.Title = $title, rec.Length = $length, rec.Disambiguation = $disambiguation "
                + "ON MATCH SET rec.Title = $title, rec.Length = $length, rec.Disambiguation = $disambiguation",
            new
            {
                id = recordingDtoToSave.Id,
                title = recordingDtoToSave.Title,
                length = recordingDtoToSave.Length,
                disambiguation = recordingDtoToSave.Disambiguation ?? string.Empty,
            }
        );

        // Store relations

        if (recordingDtoToSave.Relations != null)
        {
            foreach (var relation in recordingDtoToSave.Relations)
            {
                // Assuming TargetType will be something like "Artist", "ReleaseGroup", "Url" etc.
                // And TargetId will be the MBID of the target entity or the URL itself for Url relations.
                // The actual relation type (e.g., "cover of", "remix of") is stored in relation.Type.
                // For Url relations, we might want to create a Url node and link to it,
                // or embed the URL directly in the relationship if it's simpler and URLs are not shared.
                // For now, let's assume we are creating a relationship to an existing node of TargetType.

                // if (relation is DbRelationUrl urlRelation && urlRelation.TargetType == "Url") // Special handling for Url relations
                // {
                //     await tx.RunAsync(
                //         "MATCH (rec:Recording {Id: $recordingId}) "
                //             + "MERGE (url:Url {Resource: $resourceUrl}) "
                //             + // Create/match Url node
                //             "MERGE (rec)-[rel:HAS_URL {Type: $relationType}]->(url)", // Create relationship
                //         new
                //         {
                //             recordingId = recordingDtoToSave.Id,
                //             resourceUrl = urlRelation.Url.Resource,
                //             relationType = urlRelation.Type,
                //         }
                //     );
                // }
                // else
                // {
                //     await tx.RunAsync(
                //         $"MATCH (rec:Recording {{Id: $recordingId}}), (target:{relation.TargetType} {{Id: $targetId}}) "
                //             + $"MERGE (rec)-[rel:{relation.Type.Replace(" ", "_").ToUpper()}]->(target)",
                //         new { recordingId = recordingDtoToSave.Id, targetId = relation.TargetId }
                //     );
                // }
            }
        }
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
