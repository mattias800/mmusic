using AutoMapper;
using Hqub.MusicBrainz.Entities;
using Neo4j.Driver;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup;

public class ReleaseGroupPersistenceService(IMapper mapper)
{
    public async Task SaveReleaseGroupNodeAsync(
        IAsyncTransaction tx,
        Db.Neo4j.ServerLibrary.MusicMetaData.ReleaseGroup releaseGroupToSave
    )
    {
        await tx.RunAsync(
            "MERGE (rg:ReleaseGroup {Id: $id}) "
                + "ON CREATE SET rg.Title = $title, rg.PrimaryType = $primaryType, rg.SecondaryTypes = $secondaryTypes, rg.FirstReleaseDate = $firstReleaseDate "
                + "ON MATCH SET rg.Title = $title, rg.PrimaryType = $primaryType, rg.SecondaryTypes = $secondaryTypes, rg.FirstReleaseDate = $firstReleaseDate",
            new
            {
                id = releaseGroupToSave.Id,
                title = releaseGroupToSave.Title,
                primaryType = releaseGroupToSave.PrimaryType,
                secondaryTypes = releaseGroupToSave.SecondaryTypes,
                firstReleaseDate = releaseGroupToSave.FirstReleaseDate,
            }
        );
    }

    public async Task SaveArtistNodeAsync(
        IAsyncTransaction tx,
        Db.Neo4j.ServerLibrary.MusicMetaData.Artist artistToSave
    )
    {
        await tx.RunAsync(
            "MERGE (a:Artist {Id: $artistId}) ON CREATE SET a.Name = $name, a.SortName = $sortName, a.Gender = $gender "
                + "ON MATCH SET a.Name = $name, a.SortName = $sortName, a.Gender = $gender",
            new
            {
                artistId = artistToSave.Id,
                name = artistToSave.Name,
                sortName = artistToSave.SortName,
                gender = artistToSave.Gender,
            }
        );
    }

    public async Task SaveArtistCreditsForParentAsync(
        IAsyncTransaction tx,
        string parentEntityId,
        IEnumerable<Hqub.MusicBrainz.Entities.NameCredit> creditDtos,
        string parentLabel,
        string parentIdQueryKey,
        string relationshipType
    )
    {
        foreach (var creditDto in creditDtos)
        {
            if (creditDto.Artist == null || string.IsNullOrEmpty(creditDto.Artist.Id))
                continue;

            var artistToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Artist>(
                creditDto.Artist
            );
            await SaveArtistNodeAsync(tx, artistToSave); // Call local SaveArtistNodeAsync

            string query =
                $"MATCH (p:{parentLabel} {{Id: ${parentIdQueryKey}}}), (a:Artist {{Id: $artistId}}) "
                + $"MERGE (a)-[r:{relationshipType}]->(p) ON CREATE SET r.joinPhrase = $joinPhrase";

            var parameters = new Dictionary<string, object>
            {
                { parentIdQueryKey, parentEntityId },
                { "artistId", artistToSave.Id },
                { "joinPhrase", creditDto.JoinPhrase },
            };
            await tx.RunAsync(query, parameters);
        }
    }

    public async Task SaveReleaseNodeAsync(
        IAsyncTransaction tx,
        Db.Neo4j.ServerLibrary.MusicMetaData.Release releaseToSave
    )
    {
        await tx.RunAsync(
            "MERGE (r:Release {Id: $id}) "
                + "ON CREATE SET r.Title = $title, r.Date = $date, r.Status = $status "
                + "ON MATCH SET r.Title = $title, r.Date = $date, r.Status = $status",
            new
            {
                id = releaseToSave.Id,
                title = releaseToSave.Title,
                date = releaseToSave.Date,
                status = releaseToSave.Status,
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

    public async Task SaveRecordingNodeAsync(
        IAsyncTransaction tx,
        Db.Neo4j.ServerLibrary.MusicMetaData.Recording recordingToSave
    )
    {
        await tx.RunAsync(
            "MERGE (rec:Recording {Id: $id}) "
                + "ON CREATE SET rec.Title = $title, rec.Length = $length, rec.Disambiguation = $disambiguation "
                + "ON MATCH SET rec.Title = $title, rec.Length = $length, rec.Disambiguation = $disambiguation",
            new
            {
                id = recordingToSave.Id,
                title = recordingToSave.Title,
                length = recordingToSave.Length,
                disambiguation = recordingToSave.Disambiguation,
            }
        );
    }

    public async Task LinkRecordingToReleaseAsync(
        IAsyncTransaction tx,
        string releaseId,
        string recordingId,
        Track trackDto,
        Medium mediumDto
    )
    {
        await tx.RunAsync(
            "MATCH (rel:Release {Id: $releaseId}), (rec:Recording {Id: $recordingId}) "
                + "MERGE (rel)-[:HAS_RECORDING {trackPosition: $pos, trackTitle: $trackTitle, trackNumber: $trackNum, mediumPosition: $mediumPos, mediumFormat: $mediumFmt }]->(rec)",
            new
            {
                releaseId,
                recordingId,
                pos = trackDto.Position,
                trackTitle = trackDto.Recording.Title,
                trackNum = trackDto.Number,
                mediumPos = mediumDto.Position,
                mediumFmt = mediumDto.Format,
            }
        );
    }
}
