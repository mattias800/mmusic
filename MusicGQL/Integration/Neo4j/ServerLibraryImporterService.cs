using Hqub.MusicBrainz.Entities;
using Neo4j.Driver;

namespace MusicGQL.Integration.Neo4j;

public class ServerLibraryImporterService(
    IDriver driver,
    ILogger<ServerLibraryImporterService> logger
)
{
    public async Task SaveReleaseGroupInDatabase(ReleaseGroup releaseGroup, Release mainRelease)
    {
        await using var session = driver.AsyncSession();
        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await SaveReleaseGroupNodeAsync((IAsyncTransaction)tx, releaseGroup);
                logger.LogInformation("Saved release group {Title}", releaseGroup.Title);

                // 2. Save ReleaseGroup Artist Credits
                if (releaseGroup.Credits != null)
                {
                    await SaveArtistCreditsForParentAsync(
                        (IAsyncTransaction)tx,
                        releaseGroup.Id,
                        releaseGroup.Credits,
                        "ReleaseGroup",
                        "rgId",
                        "CREDITED_ON_RELEASE_GROUP"
                    );
                    logger.LogInformation(
                        "Saved artist credits for release group {Title}",
                        releaseGroup.Title
                    );
                }

                // 3. Process ONLY the Main Release (if found)
                if (mainRelease != null)
                {
                    await SaveReleaseNodeAsync((IAsyncTransaction)tx, mainRelease);
                    await LinkReleaseToReleaseGroupAsync(
                        (IAsyncTransaction)tx,
                        releaseGroup.Id,
                        mainRelease.Id
                    );
                    logger.LogInformation(
                        "Saved Main Release {ReleaseTitle} ({ReleaseId}) and linked to RG {RgTitle}",
                        mainRelease.Title,
                        mainRelease.Id,
                        releaseGroup.Title
                    );

                    // 4. Save Main Release Artist Credits
                    if (mainRelease.Credits != null)
                    {
                        await SaveArtistCreditsForParentAsync(
                            (IAsyncTransaction)tx,
                            mainRelease.Id,
                            mainRelease.Credits,
                            "Release",
                            "releaseId",
                            "CREDITED_ON_RELEASE"
                        );
                        logger.LogInformation(
                            "Saved artist credits for main release {Title}",
                            mainRelease.Title
                        );
                    }

                    // 5. Process Media and Tracks for the Main Release
                    if (mainRelease.Media != null)
                    {
                        foreach (var mediumDto in mainRelease.Media)
                        {
                            if (mediumDto == null)
                                continue;

                            string mediumNodeId = $"{mainRelease.Id}_m{mediumDto.Position}";
                            await SaveMediumNodeAsync(
                                (IAsyncTransaction)tx,
                                mediumNodeId,
                                mainRelease.Id,
                                mediumDto
                            );
                            logger.LogInformation(
                                "Saved Medium {MediumId} (Pos: {MediumPos}) for Main Release {ReleaseTitle}",
                                mediumNodeId,
                                mediumDto.Position,
                                mainRelease.Title
                            );

                            if (mediumDto.Tracks != null)
                            {
                                foreach (var trackDto in mediumDto.Tracks)
                                {
                                    if (
                                        trackDto?.Recording == null
                                        || string.IsNullOrEmpty(trackDto.Recording.Id)
                                    )
                                        continue;

                                    await SaveRecordingNodeAsync(
                                        (IAsyncTransaction)tx,
                                        trackDto.Recording
                                    );

                                    await LinkTrackOnMediumToRecordingAsync(
                                        (IAsyncTransaction)tx,
                                        mediumNodeId,
                                        trackDto.Recording.Id,
                                        trackDto
                                    );
                                    logger.LogInformation(
                                        "Linked Track (Pos:{TrackPos}, Num:{TrackNum}, Title:'{TrackTitle}') on Medium {MediumId} to Recording {RecordingTitle} ({RecordingId})",
                                        trackDto.Position,
                                        trackDto.Number,
                                        trackDto.Recording?.Title ?? string.Empty,
                                        mediumNodeId,
                                        trackDto.Recording?.Title ?? string.Empty,
                                        trackDto.Recording?.Id ?? string.Empty
                                    );

                                    // 6. Save Recording Artist Credits
                                    if (trackDto.Recording?.Credits != null)
                                    {
                                        await SaveArtistCreditsForParentAsync(
                                            (IAsyncTransaction)tx,
                                            trackDto.Recording.Id,
                                            trackDto.Recording.Credits,
                                            "Recording",
                                            "recordingId",
                                            "CREDITED_ON_RECORDING"
                                        );
                                        logger.LogInformation(
                                            "Saved artist credits for recording {RecordingTitle}",
                                            trackDto.Recording?.Title ?? string.Empty
                                        );
                                    }
                                }
                            }
                        }
                    }
                }
            });

            logger.LogInformation(
                "Successfully persisted data for release group {Title}",
                releaseGroup.Title
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error persisting data for release group {Title}: {ExMessage}",
                releaseGroup.Title,
                ex.Message
            );
        }
    }

    public async Task SaveReleaseGroupNodeAsync(
        IAsyncTransaction tx,
        ReleaseGroup releaseGroupToSave
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

    public async Task SaveArtistNodeAsync(IAsyncTransaction tx, Artist artistToSave)
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
        IEnumerable<NameCredit> nameCredits,
        string parentLabel,
        string parentIdQueryKey,
        string relationshipType
    )
    {
        foreach (var nameCredit in nameCredits)
        {
            if (nameCredit.Artist == null || string.IsNullOrEmpty(nameCredit.Artist.Id))
            {
                continue;
            }

            await SaveArtistNodeAsync(tx, nameCredit.Artist);

            string query =
                $"MATCH (p:{parentLabel} {{Id: ${parentIdQueryKey}}}), (a:Artist {{Id: $ArtistId}}) "
                + $"MERGE (a)-[r:{relationshipType}]->(p) "
                + $"ON CREATE SET r.JoinPhrase = $JoinPhrase, r.Name = $Name "
                + $"ON MATCH SET r.JoinPhrase = $JoinPhrase, r.Name = $Name";

            var parameters = new Dictionary<string, object>
            {
                { parentIdQueryKey, parentEntityId },
                { "ArtistId", nameCredit.Artist.Id },
                { "JoinPhrase", nameCredit.JoinPhrase ?? string.Empty },
                { "Name", nameCredit.Name ?? string.Empty },
            };

            await tx.RunAsync(query, parameters);
        }
    }

    public async Task SaveReleaseNodeAsync(IAsyncTransaction tx, Release releaseToSave)
    {
        await tx.RunAsync(
            "MERGE (r:Release {Id: $id}) "
                + "ON CREATE SET r.Title = $title, r.Date = $date, r.Status = $status, r.Barcode = $barcode, r.Country = $country, r.Quality = $quality "
                + "ON MATCH SET r.Title = $title, r.Date = $date, r.Status = $status, r.Barcode = $barcode, r.Country = $country, r.Quality = $quality",
            new
            {
                id = releaseToSave.Id,
                title = releaseToSave.Title,
                date = releaseToSave.Date,
                status = releaseToSave.Status,
                barcode = releaseToSave.Barcode,
                country = releaseToSave.Country,
                quality = releaseToSave.Quality,
            }
        );

        // Persist labels and their relationship to the release
        if (releaseToSave.Labels != null)
        {
            foreach (var labelInfo in releaseToSave.Labels)
            {
                if (labelInfo.Label == null || string.IsNullOrEmpty(labelInfo.Label.Id))
                    continue;

                // Save the Label node itself
                await SaveLabelNodeAsync(tx, labelInfo.Label);

                // Link Label to Release and set relationship properties
                await tx.RunAsync(
                    "MATCH (rel:Release {Id: $releaseId}), (lbl:Label {Id: $labelId}) "
                        + "MERGE (rel)-[rlbl:RELEASED_ON_LABEL]->(lbl) "
                        + "ON CREATE SET rlbl.CatalogNumber = $catalogNumber "
                        + "ON MATCH SET rlbl.CatalogNumber = $catalogNumber",
                    new
                    {
                        releaseId = releaseToSave.Id,
                        labelId = labelInfo.Label.Id,
                        catalogNumber = labelInfo.CatalogNumber ?? string.Empty,
                    }
                );
            }
        }
    }

    public async Task SaveLabelNodeAsync(IAsyncTransaction tx, Label label)
    {
        await tx.RunAsync(
            "MERGE (lbl:Label {Id: $id}) "
                + "ON CREATE SET lbl.Name = $name, lbl.Disambiguation = $disambiguation " // Added Disambiguation
                + "ON MATCH SET lbl.Name = $name, lbl.Disambiguation = $disambiguation",
            new
            {
                id = label.Id,
                name = label.Name,
                disambiguation = label.Disambiguation ?? string.Empty,
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
        Medium medium
    )
    {
        await tx.RunAsync(
            "MERGE (m:Medium {Id: $mediumId}) "
                + "ON CREATE SET m.Position = $position, m.Format = $format, m.TrackCount = $trackCount "
                + "ON MATCH SET m.Position = $position, m.Format = $format, m.TrackCount = $trackCount",
            new
            {
                mediumId = mediumNodeId,
                position = medium.Position,
                format = medium.Format ?? string.Empty,
                trackCount = medium.TrackCount,
            }
        );

        // Link Medium to its Release
        await tx.RunAsync(
            "MATCH (rel:Release {Id: $releaseId}), (m:Medium {Id: $mediumId}) "
                + "MERGE (rel)-[:HAS_MEDIUM]->(m)",
            new { releaseId = releaseMbId, mediumId = mediumNodeId }
        );
    }

    public async Task SaveRecordingNodeAsync(IAsyncTransaction tx, Recording recordingToSave)
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
                disambiguation = recordingToSave.Disambiguation ?? string.Empty,
            }
        );

        // Store relations

        if (recordingToSave.Relations != null)
        {
            foreach (var relation in recordingToSave.Relations)
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
        Track track
    )
    {
        await tx.RunAsync(
            "MATCH (m:Medium {Id: $mediumId}), (rec:Recording {Id: $recordingId}) "
                + "MERGE (m)-[r:INCLUDES_TRACK {Position: $trackPos, Number: $trackNum, Title: $trackTitle}]->(rec)",
            new
            {
                mediumId = mediumNodeId,
                recordingId = recordingMbId,
                trackPos = track.Position,
                trackNum = track.Number ?? string.Empty,
                trackTitle = track.Recording?.Title ?? string.Empty,
            }
        );
    }
}
