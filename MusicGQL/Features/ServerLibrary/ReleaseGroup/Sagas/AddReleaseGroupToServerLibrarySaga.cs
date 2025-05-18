using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using HotChocolate.Subscriptions;
using Microsoft.EntityFrameworkCore;
using MusicGQL.Db;
using MusicGQL.Db.Postgres;
using MusicGQL.Integration.MusicBrainz;
using Neo4j.Driver;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace MusicGQL.Features.ServerLibrary.ReleaseGroup.Sagas;

public class AddReleaseGroupToServerLibrarySaga(
    IBus bus,
    IDriver neo4jDriver,
    ILogger<AddReleaseGroupToServerLibrarySaga> logger,
    IMapper mapper,
    MusicBrainzService musicBrainzService
)
    : Saga<AddReleaseGroupToServerLibrarySagaData>,
        IAmInitiatedBy<AddReleaseGroupToServerLibrarySagaEvents.StartAddReleaseGroup>,
        IHandleMessages<AddReleaseGroupToServerLibrarySagaEvents.FoundReleaseGroupInMusicBrainz>,
        IHandleMessages<AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz>
{
    protected override void CorrelateMessages(
        ICorrelationConfig<AddReleaseGroupToServerLibrarySagaData> config
    )
    {
        config.Correlate<AddReleaseGroupToServerLibrarySagaEvents.StartAddReleaseGroup>(
            m => m.ReleaseGroupMbId,
            s => s.ReleaseGroupMbId
        );
        config.Correlate<AddReleaseGroupToServerLibrarySagaEvents.FindReleaseGroupInMusicBrainz>(
            m => m.ReleaseGroupMbId,
            s => s.ReleaseGroupMbId
        );
        config.Correlate<AddReleaseGroupToServerLibrarySagaEvents.FoundReleaseGroupInMusicBrainz>(
            m => m.ReleaseGroupMbId,
            s => s.ReleaseGroupMbId
        );
        config.Correlate<AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz>(
            m => m.ReleaseGroupMbId,
            s => s.ReleaseGroupMbId
        );
    }

    public async Task Handle(AddReleaseGroupToServerLibrarySagaEvents.StartAddReleaseGroup message)
    {
        if (!IsNew)
        {
            return;
        }

        logger.LogInformation(
            "Starting AddReleaseGroupToServerLibrarySaga for {ReleaseGroupMbId}",
            message.ReleaseGroupMbId
        );

        Data.StatusDescription = "Looking up release group in MusicBrainz";
        await bus.Send(
            new AddReleaseGroupToServerLibrarySagaEvents.FindReleaseGroupInMusicBrainz(
                message.ReleaseGroupMbId
            )
        );
    }

    public async Task Handle(
        AddReleaseGroupToServerLibrarySagaEvents.FoundReleaseGroupInMusicBrainz message
    )
    {
        logger.LogInformation(
            $"Processing ReleaseGroup {message.ReleaseGroupMbId} and associated data for Neo4j."
        );

        await using var session = neo4jDriver.AsyncSession();

        try
        {
            // 1. Save ReleaseGroup
            var releaseGroupToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.ReleaseGroup>(
                message.ReleaseGroup
            );
            await session.ExecuteWriteAsync(async tx =>
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

                // 2. Save ReleaseGroup Artist Credits
                if (message.ReleaseGroup.Credits != null)
                {
                    foreach (var creditDto in message.ReleaseGroup.Credits)
                    {
                        if (creditDto.Artist == null || string.IsNullOrEmpty(creditDto.Artist.Id)) continue;
                        var artistToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Artist>(
                            creditDto.Artist
                        );
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
                        await tx.RunAsync(
                            "MATCH (rg:ReleaseGroup {Id: $rgId}), (a:Artist {Id: $artistId}) "
                                + "MERGE (a)-[r:CREDITED_ON_RELEASE_GROUP]->(rg) ON CREATE SET r.joinPhrase = $joinPhrase",
                            new
                            {
                                rgId = releaseGroupToSave.Id,
                                artistId = artistToSave.Id,
                                joinPhrase = creditDto.JoinPhrase,
                            }
                        );
                    }
                }
            });
            logger.LogInformation(
                $"ReleaseGroup {releaseGroupToSave.Id} and its main artist credits saved/updated in Neo4j"
            );

            // 3. Fetch and Process Releases for the Release Group
            var releaseDtos = await musicBrainzService.GetReleasesForReleaseGroupAsync(
                releaseGroupToSave.Id
            );
            if (releaseDtos != null)
            {
                foreach (var releaseDto in releaseDtos)
                {
                    var releaseToSave = mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Release>(
                        releaseDto
                    );
                    await session.ExecuteWriteAsync(async tx =>
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
                        // Link Release to ReleaseGroup
                        await tx.RunAsync(
                            "MATCH (rg:ReleaseGroup {Id: $rgId}), (r:Release {Id: $releaseId}) "
                                + "MERGE (r)-[:RELEASE_OF]->(rg)",
                            new { rgId = releaseGroupToSave.Id, releaseId = releaseToSave.Id }
                        );

                        // 4. Save Release Artist Credits
                        if (releaseDto.Credits != null)
                        {
                            foreach (var creditDto in releaseDto.Credits)
                            {
                                if (
                                    creditDto.Artist == null
                                    || string.IsNullOrEmpty(creditDto.Artist.Id)
                                )
                                    continue;
                                var artistToSave =
                                    mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Artist>(
                                        creditDto.Artist
                                    );
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
                                await tx.RunAsync(
                                    "MATCH (rel:Release {Id: $releaseId}), (a:Artist {Id: $artistId}) "
                                        + "MERGE (a)-[r:CREDITED_ON_RELEASE]->(rel) ON CREATE SET r.joinPhrase = $joinPhrase",
                                    new
                                    {
                                        releaseId = releaseToSave.Id,
                                        artistId = artistToSave.Id,
                                        joinPhrase = creditDto.JoinPhrase,
                                    }
                                );
                            }
                        }

                        // 5. Process Media and Tracks for each Release
                        if (releaseDto.Media != null)
                        {
                            foreach (var mediumDto in releaseDto.Media)
                            {
                                // For simplicity, medium properties can be part of Release or Track if not complex.
                                // If Medium needs to be a node: MERGE (m:Medium {Id: ...}) SET m.Position = mediumDto.Position, m.Format = mediumDto.Format
                                // MATCH (r:Release {Id: releaseToSave.Id}) MERGE (r)-[:HAS_MEDIUM]->(m)

                                if (mediumDto.Tracks != null)
                                {
                                    foreach (var trackDto in mediumDto.Tracks)
                                    {
                                        if (
                                            trackDto.Recording == null
                                            || string.IsNullOrEmpty(trackDto.Recording.Id)
                                        )
                                            continue;
                                        var recordingToSave =
                                            mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Recording>(
                                                trackDto.Recording
                                            );
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

                                        // Link Recording to Release (perhaps via a Track node if Track has more meaning/properties or its own ID)
                                        // For now, directly linking Recording to Release for simplicity, implying track info might be on relationship or recording itself.
                                        // A more detailed model might be: (Release)-[:HAS_TRACK {position: trackDto.Position, title: trackDto.Title }]->(Recording)
                                        await tx.RunAsync(
                                            "MATCH (rel:Release {Id: $releaseId}), (rec:Recording {Id: $recordingId}) "
                                                + "MERGE (rel)-[:HAS_RECORDING {trackPosition: $pos, trackTitle: $trackTitle, trackNumber: $trackNum, mediumPosition: $mediumPos, mediumFormat: $mediumFmt }]->(rec)",
                                            new
                                            {
                                                releaseId = releaseToSave.Id,
                                                recordingId = recordingToSave.Id,
                                                pos = trackDto.Position,
                                                trackTitle = trackDto.Recording.Title,
                                                trackNum = trackDto.Number,
                                                mediumPos = mediumDto.Position,
                                                mediumFmt = mediumDto.Format,
                                            }
                                        );

                                        // 6. Save Recording Artist Credits
                                        if (trackDto.Recording.Credits != null)
                                        {
                                            foreach (
                                                var creditDto in trackDto
                                                    .Recording
                                                    .Credits
                                            )
                                            {
                                                if (
                                                    creditDto.Artist == null
                                                    || string.IsNullOrEmpty(creditDto.Artist.Id)
                                                )
                                                    continue;
                                                var artistToSave =
                                                    mapper.Map<Db.Neo4j.ServerLibrary.MusicMetaData.Artist>(
                                                        creditDto.Artist
                                                    );
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
                                                await tx.RunAsync(
                                                    "MATCH (rec:Recording {Id: $recordingId}), (a:Artist {Id: $artistId}) "
                                                        + "MERGE (a)-[r:CREDITED_ON_RECORDING]->(rec) ON CREATE SET r.joinPhrase = $joinPhrase",
                                                    new
                                                    {
                                                        recordingId = recordingToSave.Id,
                                                        artistId = artistToSave.Id,
                                                        joinPhrase = creditDto.JoinPhrase,
                                                    }
                                                );
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
                    logger.LogInformation(
                        $"Release {releaseToSave.Id} and its data saved/updated in Neo4j"
                    );
                }
            }

            MarkAsComplete();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing ReleaseGroup {MessageReleaseGroupMbId} for Neo4j: {ExMessage}",
                message.ReleaseGroupMbId,
                ex.Message
            );
            MarkAsComplete(); // Ensure saga completes even on error
        }
    }

    public Task Handle(
        AddReleaseGroupToServerLibrarySagaEvents.DidNotFindReleaseGroupInMusicBrainz message
    )
    {
        logger.LogInformation(
            $"ReleaseGroup {message.ReleaseGroupMbId} not found in MusicBrainz, completing saga."
        );
        MarkAsComplete();
        return Task.CompletedTask;
    }
}
